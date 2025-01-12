using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Malarkey.Application.Security;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Malarkey.Abstractions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Components;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>, IMalarkeyServerAuthenticationCallbackHandler
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionHandler _sessionHandler;
    private readonly IReadOnlyDictionary<MalarkeyIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly MalarkeyIntegrationConfiguration _intConf;
    private readonly ILogger<MalarkeyServerAuthenticationHandler> _logger;

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>

    private MalarkeyAuthenticationEvents _events;
    protected new MalarkeyAuthenticationEvents Events => _events;

    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyServerAuthenticationHandlerOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IMalarkeyTokenHandler tokenHandler,
        IMalarkeyAuthenticationSessionHandler sessionHandler,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IMalarkeyProfileRepository profileRepo,
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        ILogger<MalarkeyServerAuthenticationHandler> logger) : base(options, loggerFactory, encoder)
    {
        _logger = logger;
        _events = new MalarkeyAuthenticationEvents();
        _intConf = intConf.Value;
        _tokenHandler = tokenHandler;
        _sessionHandler = sessionHandler;
        _flowHandlers = flowHandlers
            .ToDictionarySafe(_ => _.HandlerFor);
        _profileRepo = profileRepo;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() => (await _tokenHandler.ExtractProfileAndIdentities(Context)) switch
        {
            null => AuthenticateResult.Fail("No profile info found"),
            MalarkeyProfileAndIdentities p => AuthenticateResult.Success(new AuthenticationTicket(
                principal: p.Profile.ToClaimsPrincipal(p.Identities),
                MalarkeyConstants.MalarkeyAuthenticationScheme
                ))
        };



    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var forwarder = Request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        var scopes = Request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName)
            .Select(_ => _.Value.ToString().Split(" "))
            .FirstOrDefault();
        var forwarderState = Request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderStateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        var audience = ExtractPublicKeyOfReceiver();
        var idp = ExtractIdentityProvider();
        if(idp == null || !_flowHandlers.TryGetValue(idp.Value, out var flowHandler))
        {
            var accessDeniedUrl = BuildRedirectUri(Options.AccessDeniedUrl);
            var redirContext = new RedirectContext<MalarkeyServerAuthenticationHandlerOptions>(
                context: Context,
                scheme: Scheme,
                options: Options,
                properties: properties,
                redirectUri: accessDeniedUrl);
            await Events.OnRedirectToAccessDenied(redirContext);
        }
        else
        {
            var session = await _sessionHandler.InitSession(idp.Value, forwarder, audience, scopes, forwarderState);
            var redirectUrl = flowHandler.ProduceAuthorizationUrl(session);
            var redirContext = new RedirectContext<MalarkeyServerAuthenticationHandlerOptions>(
                context: Context,
                scheme: Scheme,
                options: Options,
                properties: properties,
                redirectUri: redirectUrl);
            await Events.OnRedirectToLogin(redirContext);

        }

    }

    public async Task<IResult> HandleCallback(HttpRequest request)
    {
        IMalarkeyOAuthFlowHandler.RedirectData? redirData = null;
        foreach(var handler in _flowHandlers.Values)
        {
            redirData = await handler.ExtractRedirectData(request);
            if (!string.IsNullOrWhiteSpace(redirData?.State))
                break;
        }
        if (string.IsNullOrWhiteSpace(redirData?.State))
        {
            _logger.LogError($"Unable to extract state from callback request");
            return await ReturnError("Could not extract state");
        }
        var session = await _sessionHandler.SessionForState(redirData.State);
        if(session == null)
        {
            _logger.LogError($"Unable to load session from state: {redirData.State}");
            return await ReturnError($"Could not find session for state={redirData.State}");
        }
        var flowHandler = _flowHandlers[session.IdProvider];
        var identity = await flowHandler.ResolveIdentity(session, redirData);
        if (identity == null)
        {
            _logger.LogError($"Unable to load identity from state: {redirData.State} and redirect data: {redirData.ToString()}");
            return await ReturnError($"Could not resolve identity by {session.IdProvider} for callback request with URL={request.GetDisplayUrl()}");
        }
        var profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
        if (profileAndIdentities == null)
        {
            _logger.LogError($"Unable to load/create profile and/or identities for identity {identity.ProvidersIdForIdentity} from provider: {identity.IdentityProvider}");
            return await ReturnError($"Could not locate profile identity: {identity.ProfileId} by {session.IdProvider}");
        }
        _logger.LogInformation($"Resolved profile: {profileAndIdentities.Profile.ProfileId} and {profileAndIdentities.Identities.Count} identities");

        identity = profileAndIdentities.Identities
            .Where(_ => _.ProviderId == identity.ProviderId)
            .First();
        var (profileToken, profileTokenString) = await _tokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        var (identityToken, identityTokenString) = await _tokenHandler.IssueToken(identity, session.Audience);
        await _sessionHandler.UpdateSessionWithTokenInfo(session, profileToken, identityToken);
        _tokenHandler.BakeCookies(request.HttpContext, profileToken, [identityToken]);
        var redirectUrl = session.Forwarder ?? $"/profile";
        _logger.LogInformation($"Will redirect to URL: {redirectUrl} ");
        var redirect = new MalarkeyAuthenticationSuccessHttpResult(
            RedirectUrl: redirectUrl,
            ProfileToken: profileTokenString,
            IdentityToken: identityTokenString,
            IdentityProviderAccessToken: identity.IdentityProviderTokenToUse?.Token,
            ForwarderState: session.ForwarderState,
            Logger: _logger
        );
        _logger.LogInformation($"Created redirect result with forward URL: {redirect.ForwardLocation} ");
        return redirect;
    }

    private Task<BadRequest<string>> ReturnError(string errorMessage) => Task.FromResult(
        TypedResults.BadRequest(errorMessage)
        );

    private MalarkeyIdentityProvider? ExtractIdentityProvider()
    {
        var inHeaders = Request.Headers.TryGetValue(MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName, out var idpHeaderString);
        var fromQuery = Request.Query
            .Where(_ => _.Key.ToLower() == MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName.ToLower())
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();

        var idpList = new List<MalarkeyIdentityProvider> {
            MalarkeyIdentityProvider.Microsoft,
            MalarkeyIdentityProvider.Google,
            MalarkeyIdentityProvider.Facebook,
            MalarkeyIdentityProvider.Spotify
        }.Select(_ => (Key: _.ToString().ToLower(), Value: _))
        .Where(_ => _.Key == (inHeaders ? idpHeaderString.ToString().ToLower() : fromQuery?.ToLower()))
        .Select(_ => _.Value)
        .ToList();
        if (!idpList.Any())
            return null;

        return idpList.First();
    }


    private string ExtractPublicKeyOfReceiver()
    {
        if (!Request.Headers.TryGetValue(MalarkeyConstants.Authentication.AudienceHeaderName, out var pubKey))
            return _intConf.PublicKey;
        return pubKey.ToString();
    }



}
