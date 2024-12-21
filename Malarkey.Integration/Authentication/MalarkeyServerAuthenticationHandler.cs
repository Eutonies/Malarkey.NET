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

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>, IMalarkeyServerAuthenticationCallbackHandler
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionHandler _sessionHandler;
    private readonly MalarkeyServerAuthenticationHandlerOptions _options;
    private readonly IReadOnlyDictionary<MalarkeyOAuthIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly MalarkeyIntegrationConfiguration _intConf;

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new MalarkeyAuthenticationEvents Events
    {
        get { return (MalarkeyAuthenticationEvents) base.Events!; }
        set { base.Events = value; }
    }

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
        _intConf = intConf.Value;
        _tokenHandler = tokenHandler;
        _sessionHandler = sessionHandler;
        _options = options.CurrentValue;
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
        var idp = Request.Headers.TryGetValue(IntegrationConstants.IdProviderHeaderName, out var idpString) ? (idpString.ToString() switch
        {
            IntegrationConstants.MalarkeyIdProviders.Microsoft => (MalarkeyOAuthIdentityProvider?)MalarkeyOAuthIdentityProvider.Microsoft,
            IntegrationConstants.MalarkeyIdProviders.Google => MalarkeyOAuthIdentityProvider.Google,
            IntegrationConstants.MalarkeyIdProviders.Facebook => MalarkeyOAuthIdentityProvider.Facebook,
            IntegrationConstants.MalarkeyIdProviders.Spotify => MalarkeyOAuthIdentityProvider.Spotify,
            _ => null
        }) : null;
        if(idp == null || !_flowHandlers.TryGetValue(idp.Value, out var flowHandler))
        {
            var accessDeniedUrl = BuildRedirectUri(_options.AccessDeniedUrl);
            var redirContext = new RedirectContext<MalarkeyServerAuthenticationHandlerOptions>(
                context: Context,
                scheme: Scheme,
                options: _options,
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
                options: _options,
                properties: properties,
                redirectUri: redirectUrl);
            await Events.OnRedirectToLogin(redirContext);

        }

    }

    public async Task<MalarkeyServerAuthenticationResult> HandleCallback(HttpRequest request)
    {
        string? state = null;
        foreach(var handler in _flowHandlers.Values)
        {
            state = handler.StateFrom(request);
            if (!string.IsNullOrWhiteSpace(state))
                break;
        }
        if (string.IsNullOrWhiteSpace(state))
            return new MalarkeyServerAuthenticationFailureResult("Could not extract state");
        var session = await _sessionHandler.SessionForState(state);
        if(session == null)
            return new MalarkeyServerAuthenticationFailureResult($"Could not find session for state={state}");
        var flowHandler = _flowHandlers[session.IdProvider];
        var identity = await flowHandler.ResolveIdentity(session, request);
        if (identity == null)
            return new MalarkeyServerAuthenticationFailureResult($"Could not resolve identity by {session.IdProvider} for callback request with URL={request.GetDisplayUrl()}");
        var profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
        if (profileAndIdentities == null)
            return new MalarkeyServerAuthenticationFailureResult($"Could not locate profile identity: {identity.ProfileId} by {session.IdProvider}");
        identity = profileAndIdentities.Identities
            .Where(_ => _.ProviderId == identity.ProviderId)
            .First();
        var (profileToken, profileTokenString) = await _tokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        var (identityToken, identityTokenString) = await _tokenHandler.IssueToken(identity, session.Audience);
        await _sessionHandler.UpdateSessionWithTokenInfo(session, profileToken, identityToken);
        var redirectUrl = session.Forwarder ?? $"{_intConf.ServerBasePath}/";
        var props = new AuthenticationProperties();
        var redirectContext = new RedirectContext<MalarkeyServerAuthenticationHandlerOptions>(Context, Scheme, _options, props, redirectUrl);
        await Events.OnRedirectUponCompletion(redirectContext);

    }


    private string ExtractPublicKeyOfReceiver()
    {
        if (!Request.Headers.TryGetValue(IntegrationConstants.TokenReceiverHeaderName, out var pubKey))
            return _options.PublicKey;
        return pubKey.ToString();
    }


}
