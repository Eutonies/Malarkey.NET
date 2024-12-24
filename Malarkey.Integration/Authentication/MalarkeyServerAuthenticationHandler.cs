using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>, IMalarkeyServerAuthenticationCallbackHandler
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionHandler _sessionHandler;
    private readonly IReadOnlyDictionary<MalarkeyOAuthIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly MalarkeyIntegrationConfiguration _intConf;

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>

    private MalarkeyAuthenticationEvents _events;
    protected new MalarkeyAuthenticationEvents Events => _events;

    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyServerAuthenticationHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IMalarkeyTokenHandler tokenHandler,
        IMalarkeyAuthenticationSessionHandler sessionHandler,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IMalarkeyProfileRepository profileRepo,
        IOptions<MalarkeyIntegrationConfiguration> intConf
        ) : base(options, logger, encoder)
    {
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
                IntegrationConstants.MalarkeyAuthenticationScheme
                ))
        };



    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var requestedUrl = OriginalPath;
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
            var session = await _sessionHandler.InitSession(idp.Value, requestedUrl, audience);
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
            return await ReturnError("Could not extract state");
        }
        var session = await _sessionHandler.SessionForState(redirData.State);
        if(session == null)
        {
            return await ReturnError($"Could not find session for state={redirData.State}");
        }
        var flowHandler = _flowHandlers[session.IdProvider];
        var identity = await flowHandler.ResolveIdentity(session, redirData);
        if (identity == null)
        {
            return await ReturnError($"Could not resolve identity by {session.IdProvider} for callback request with URL={request.GetDisplayUrl()}");
        }
        var profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
        if (profileAndIdentities == null)
        {
            return await ReturnError($"Could not locate profile identity: {identity.ProfileId} by {session.IdProvider}");
        }
        identity = profileAndIdentities.Identities
            .Where(_ => _.ProviderId == identity.ProviderId)
            .First();
        var (profileToken, profileTokenString) = await _tokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        var (identityToken, identityTokenString) = await _tokenHandler.IssueToken(identity, session.Audience);
        await _sessionHandler.UpdateSessionWithTokenInfo(session, profileToken, identityToken);
        var redirectUrl = session.Forwarder ?? $"{_intConf.ServerBasePath}/";
        var redirect = TypedResults.Redirect(redirectUrl);
        return redirect;
    }

    private Task<BadRequest<string>> ReturnError(string errorMessage) => Task.FromResult(
        TypedResults.BadRequest(errorMessage)
        );

    private MalarkeyOAuthIdentityProvider? ExtractIdentityProvider()
    {
        var inHeaders = Request.Headers.TryGetValue(IntegrationConstants.IdProviderHeaderName, out var idpHeaderString);
        var fromQuery = Request.Query
            .Where(_ => _.Key.ToLower() == IntegrationConstants.IdProviderHeaderName.ToLower())
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();

        var idp = ( inHeaders ? idpHeaderString.ToString() : fromQuery)?.ToLower() switch
        {
            IntegrationConstants.MalarkeyIdProviders.Microsoft => (MalarkeyOAuthIdentityProvider?)MalarkeyOAuthIdentityProvider.Microsoft,
            IntegrationConstants.MalarkeyIdProviders.Google => MalarkeyOAuthIdentityProvider.Google,
            IntegrationConstants.MalarkeyIdProviders.Facebook => MalarkeyOAuthIdentityProvider.Facebook,
            IntegrationConstants.MalarkeyIdProviders.Spotify => MalarkeyOAuthIdentityProvider.Spotify,
            _ => null
        };
        return idp;
    }


    private string ExtractPublicKeyOfReceiver()
    {
        if (!Request.Headers.TryGetValue(IntegrationConstants.TokenReceiverHeaderName, out var pubKey))
            return _intConf.PublicKey;
        return pubKey.ToString();
    }



}
