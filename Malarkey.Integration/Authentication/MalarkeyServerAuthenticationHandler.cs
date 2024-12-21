using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionHandler _sessionHandler;
    private readonly MalarkeyServerAuthenticationHandlerOptions _options;
    private readonly IReadOnlyDictionary<MalarkeyOAuthIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;

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
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers
        ) : base(options, logger, encoder)
    {
        _tokenHandler = tokenHandler;
        _sessionHandler = sessionHandler;
        _options = options.CurrentValue;
        _flowHandlers = flowHandlers
            .ToDictionarySafe(_ => _.HandlerFor);
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
            var session = await _sessionHandler.InitSession(idp.Value, requestedUrl);
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





}
