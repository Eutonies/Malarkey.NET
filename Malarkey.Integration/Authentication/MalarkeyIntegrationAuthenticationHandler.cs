using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Malarkey.Abstractions;
using Malarkey.Abstractions.Token;
using Azure.Core;
using System.Security.Principal;

namespace Malarkey.Integration.Authentication;
public class MalarkeyIntegrationAuthenticationHandler : AuthenticationHandler<MalarkeyIntegrationAuthenticationHandlerOptions>, IMalarkeyAuthenticationCallbackHandler
{
    private readonly IMalarkeyAuthenticationSessionCache _sessionCache;
    private readonly IReadOnlyDictionary<MalarkeyIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly ILogger<MalarkeyIntegrationAuthenticationHandler> _logger;
    private readonly string _malarkeyTokenReceiver;
    private readonly MalarkeySynchronizer _synchronizer;
    private readonly IMalarkeyTokenIssuer _tokenIssuer;


    public MalarkeyIntegrationAuthenticationHandler(
        IOptionsMonitor<MalarkeyIntegrationAuthenticationHandlerOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        MalarkeySynchronizer synchronizer,
        IMalarkeyTokenIssuer tokenHandler,
        IMalarkeyAuthenticationSessionCache sessionCache,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        ILogger<MalarkeyIntegrationAuthenticationHandler> logger) : base(options, loggerFactory, encoder)
    {
        _synchronizer = synchronizer;
        _logger = logger;
        _tokenIssuer = tokenHandler;
        _sessionCache = sessionCache;
        _flowHandlers = flowHandlers
            .ToDictionarySafe(_ => _.HandlerFor);
        _malarkeyTokenReceiver = intConf.Value.PublicKey.CleanCertificate();
    }

    private string TokenReceiver => (Context?.Request?.Headers?.TryGetValue(MalarkeyConstants.Authentication.AudienceHeaderName, out var audience) ?? false) ?
        audience.ToString() :
        _malarkeyTokenReceiver;


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var profileAndIdentities = await _tokenIssuer.ExtractProfileAndIdentities(Context, TokenReceiver);
        if (IsBlazorRequest){
            DetailLog("Identified as BlazorRequest");
            return AuthenticateResult.NoResult();
        }
        else if(profileAndIdentities == null) {
            DetailLog($"No profile found in cookies");
            return AuthenticateResult.Fail("No profile found in cookies");
        }
        var authSession = await LoadSession();
        if (authSession != null && !IsBlazorRequest)
        {
            if (authSession.AlwaysChallenge) {
                DetailLog("Asked to always challange");
                return AuthenticateResult.Fail("Asked to always challenge");
            }
            if (authSession.RequestedIdProvider != null && !profileAndIdentities.Identities.Any(_ => _.IdentityProvider == authSession.RequestedIdProvider)) {
                DetailLog($"No identity token for: {authSession.RequestedIdProvider} found");
                return AuthenticateResult.Fail($"No identity token for: {authSession.RequestedIdProvider} found");

            }
        }
        var (prof, idents, _) = profileAndIdentities;
        var returnee = AuthenticateResult.Success(new AuthenticationTicket(
                principal: prof.ToClaimsPrincipal(idents),
                MalarkeyConstants.MalarkeyAuthenticationScheme
                ));
        DetailLog($"Returning profile with ID: {prof.ProfileId} and identities for: {idents.Select(_ => _.IdentityProvider.ToString()).MakeString(",")}");        
        return returnee;

    }

    private void DetailLog(string mess) {
        DebugLog(mess);
        DebugLog($"  Request: {Request.GetDisplayUrl}");
        DebugLog($"  Headers present: {Request.Headers.Select(_ => _.Key).Order().MakeString(",")}");
        DebugLog($"  Cookies present: {Request.Cookies.Select(_ => _.Key).Order().MakeString(",")}");

    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authSession = await LoadSession();
        if(
            authSession == null || 
            authSession.RequestedIdProvider == null || 
            !_flowHandlers.TryGetValue(authSession.RequestedIdProvider.Value, out var flowHandler))
        {
            var pars = Request.ResolveParameters();
            var url = pars.ProduceAuthenticationString();
            RedirectTo(url);
            return;
        }
        if(authSession.IdpSession == null)
        {
            await _synchronizer.PerformLockedActionAsync<MalarkeyIntegrationAuthenticationHandler>(async () =>
            {
                authSession = await LoadSession();
                if (authSession?.IdpSession == null)
                {
                    var idpSession = flowHandler.PopulateIdpSession(authSession!);
                    authSession = await _sessionCache.InitiateIdpSession(authSession!.SessionId, idpSession);
                }
            });
        }
        var redirectUrl = flowHandler.ProduceAuthorizationUrl(authSession, authSession.IdpSession!);
        RedirectTo(redirectUrl);
    }

    public async Task<IResult> HandleCallback(HttpRequest request)
    {
        DebugLog($"Authentication callback called: {request.Method} {request.GetDisplayUrl()}");
        IMalarkeyOAuthFlowHandler.RedirectData? redirData = null;
        foreach(var handler in _flowHandlers.Values)
        {
            redirData = await handler.ExtractRedirectData(request);
            if (!string.IsNullOrWhiteSpace(redirData?.State)) {
                DebugLog($"Handler for: {handler.HandlerFor} parsed redirect data to:\n {redirData.ToPropertiesString()}");
                break;
            }
        }
        if (string.IsNullOrWhiteSpace(redirData?.State))
        {
            _logger.LogError($"Unable to extract state from callback request");
            return await ReturnError("Could not extract state");
        }
        var session = await _sessionCache.LoadByState(redirData.State);
        if(session == null)
        {
            _logger.LogError($"Unable to load session from state: {redirData.State}");
            return await ReturnError($"Could not find session for state={redirData.State}");
        }
        DebugLog($"Loaded session: {session.ToPropertiesString()}");
        if(session.RequestedIdProvider == null || !_flowHandlers.TryGetValue(session.RequestedIdProvider.Value, out var flowHandler))
        {
            _logger.LogError($"Unable to determine flow handler on callback from state: {redirData.State}, requested ID provider: {session.RequestedIdProvider}");
            return await ReturnError($"Could not find flow handler on callback for state={redirData.State}");
        }
        var identity = await flowHandler.ResolveIdentity(session, redirData);
        if (identity == null)
        {
            _logger.LogError($"Unable to load identity from state: {redirData.State} and redirect data: {redirData.ToString()}");
            return await ReturnError($"Could not resolve identity by {session.RequestedIdProvider} for callback request with URL={request.GetDisplayUrl()}");
        }
        DebugLog($"Resolved identity: {identity.ToPropertiesString()}");
        MalarkeyProfileAndIdentities? profileAndIdentities = await ConstructProfile(session, identity);
        if (profileAndIdentities == null)
        {
            _logger.LogError($"Unable to load/create profile and/or identities for identity {identity.ProvidersIdForIdentity} from provider: {identity.IdentityProvider}");
            return await ReturnError($"Could not resolve profile from identity provider ID: {identity.ProvidersIdForIdentity} by {session.RequestedIdProvider}");
        }
        _logger.LogInformation($"Resolved profile: {profileAndIdentities.Profile.ProfileId} and {profileAndIdentities.Identities.Count} identities");
        identity = profileAndIdentities.Identities
            .First(_ => _.IdentityProvider == identity.IdentityProvider && _.ProviderId == identity.ProviderId);

        var (profileTokenString, allIdentityTokenStrings) = await HandleTokenIssuance(profileAndIdentities, session, request.HttpContext, identity);
        PublisizeRegistrationEvent(identity);

        var returnee = new MalarkeyAuthenticationSuccessHttpResult(
            Session: session,
            ProfileToken: profileTokenString,
            IdentityTokens: allIdentityTokenStrings,
            Logger: _logger
            );
        return returnee;
    }

    protected virtual void PublisizeRegistrationEvent(MalarkeyProfileIdentity ident) { }


    protected virtual async Task<MalarkeyProfileAndIdentities> ConstructProfile(MalarkeyAuthenticationSession session, MalarkeyProfileIdentity identity)
    {
        var profile = MalarkeyProfile.With(identity.PreferredNameToUse ?? identity.FirstName, identity.FirstName, identity.LastName, primaryEmail: identity.EmailToUse);
        var returnee = new MalarkeyProfileAndIdentities(profile, [identity]);
        await Task.CompletedTask;
        return returnee;
    }

    protected virtual async Task<(string ProfileToken, IReadOnlyCollection<string> IdentityTokens)> HandleTokenIssuance(
        MalarkeyProfileAndIdentities profileAndIdentities, 
        MalarkeyAuthenticationSession session,
        HttpContext context,
        MalarkeyProfileIdentity identity)
    {
        var (profileToken, profileTokenString) = await _tokenIssuer.IssueToken(profileAndIdentities.Profile, session.Audience);
        var existingIdentityTokens = (await _tokenIssuer.ValidateIdentityTokens(context, session.Audience)).Results
            .Select(_ => _ as MalarkeyTokenValidationSuccessResult)
            .Where(_ => _ != null)
            .Select(_ => (Identity: (_!.ValidToken as MalarkeyIdentityToken)!.Identity, TokenString: _.TokenString))
            .ToList();
        var existingIdentityTokensToCarry = existingIdentityTokens
            .Where(_ => _.Identity.IdentityId != identity.IdentityId)
            .ToList();
        var existingIdentityTokenIdentityIds = existingIdentityTokensToCarry
            .Select(_ => _.Identity.IdentityId)
            .ToHashSet();
        var identitiesForTokenIssue = profileAndIdentities.Identities
            .Where(_ => !existingIdentityTokenIdentityIds.Contains(_.IdentityId))
            .ToList();
        var newlyIssuedIdentityTokens = await _tokenIssuer.IssueTokens(identitiesForTokenIssue, session.Audience);
        var tokenForRelevantIdentity = newlyIssuedIdentityTokens
            .First(_ => _.Token.Identity.IdentityId == identity.IdentityId);
        await _sessionCache.UpdateSessionWithTokenInfo(session, profileToken, tokenForRelevantIdentity.Token);
        var allIdentityTokens = existingIdentityTokensToCarry
            .Select(_ => _.TokenString)
            .Union(
               newlyIssuedIdentityTokens
                 .Select(_ => _.TokenString)
            ).ToList();
        return (profileTokenString, allIdentityTokens);
    }


    private Task<BadRequest<string>> ReturnError(string errorMessage) => Task.FromResult(
        TypedResults.BadRequest(errorMessage)
        );



    private void RedirectTo(string url)
    {
        Context.Response.StatusCode = 302;
        Context.Response.Redirect(url, permanent: false);
    }

    private async Task<MalarkeyAuthenticationSession?> LoadSession()
    {
        var sessionState = Request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.SessionStateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (sessionState == null)
            return null;
        var returnee = await _sessionCache.LoadByState(sessionState);
        return returnee;
    }

    private bool IsBlazorRequest => Request.Path.ToString().ToLower().StartsWith("/_blazor");


    private void DebugLog(string str) => 
       _logger.LogInformation(str);


}
