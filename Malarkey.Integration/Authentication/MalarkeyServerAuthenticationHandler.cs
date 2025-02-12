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
using Malarkey.Abstractions;
using Malarkey.Application.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Configuration;
using Malarkey.Abstractions.Common;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>, IMalarkeyServerAuthenticationCallbackHandler
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionRepository _sessionRepo;
    private readonly IReadOnlyDictionary<MalarkeyIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly ILogger<MalarkeyServerAuthenticationHandler> _logger;
    private readonly string _malarkeyTokenReceiver;
    private readonly IMalarkeyServerAuthenticationEventHandler _events;
    private readonly MalarkeySynchronizer _synchronizer;


    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyServerAuthenticationHandlerOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        MalarkeySynchronizer synchronizer,
        IMalarkeyTokenHandler tokenHandler,
        IMalarkeyAuthenticationSessionRepository sessionRepo,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IMalarkeyProfileRepository profileRepo,
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        IOptions<MalarkeyApplicationConfiguration> appConf,
        ILogger<MalarkeyServerAuthenticationHandler> logger,
        IMalarkeyServerAuthenticationEventHandler events) : base(options, loggerFactory, encoder)
    {
        _synchronizer = synchronizer;
        _events = events;
        _logger = logger;
        _tokenHandler = tokenHandler;
        _sessionRepo = sessionRepo;
        _flowHandlers = flowHandlers
            .ToDictionarySafe(_ => _.HandlerFor);
        _profileRepo = profileRepo;
        _malarkeyTokenReceiver = appConf.Value.Certificate.PublicKeyPem.CleanCertificate();
    }

    private string TokenReceiver => (Context?.Request?.Headers?.TryGetValue(MalarkeyConstants.AuthenticationRequestQueryParameters.ClientPublicKey, out var audience) ?? false) ?
        audience.ToString().CleanCertificate() :
        _malarkeyTokenReceiver;


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authAttribute = ExtractAttribute();
        if (authAttribute == null)
        {
            DetailLog($"No authorization attribute found, so will not invalidate request");
            return AuthenticateResult.NoResult();
        }

        var profileAndIdentities = await _tokenHandler.ExtractProfileAndIdentities(Context, TokenReceiver);
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



    private MalarkeyAuthenticationAttribute? ExtractAttribute()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint == null)
            return null;
        var authAttribute = endpoint.Metadata
            .OfType<MalarkeyAuthenticationAttribute>()
            .FirstOrDefault();
        return authAttribute;

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
            await _synchronizer.PerformLockedActionAsync<MalarkeyServerAuthenticationHandler>(async () =>
            {
                authSession = await LoadSession();
                if (authSession?.IdpSession == null)
                {
                    var idpSession = flowHandler.PopulateIdpSession(authSession!);
                    authSession = await _sessionRepo.InitiateIdpSession(authSession!.SessionId, idpSession);
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
            return new MalarkeyHttpErrorMessageResult($"Unable to extract state from callback request");
        }
        var session = await _sessionRepo.LoadByState(redirData.State);
        if(session == null)
        {
            _logger.LogError($"Unable to load session from state: {redirData.State}");
            return new MalarkeyHttpErrorMessageResult($"Unable to load session from state: {redirData.State}");
        }
        DebugLog($"Loaded session: {session.ToPropertiesString()}");
        if(session.RequestedIdProvider == null || !_flowHandlers.TryGetValue(session.RequestedIdProvider.Value, out var flowHandler))
        {
            _logger.LogError($"Unable to determine flow handler on callback from state: {redirData.State}, requested ID provider: {session.RequestedIdProvider}");
            return new MalarkeyHttpErrorMessageResult($"Unable to determine flow handler on callback from state: {redirData.State}, requested ID provider: {session.RequestedIdProvider}");
        }
        var identity = await flowHandler.ResolveIdentity(session, redirData);
        if (identity == null)
        {
            _logger.LogError($"Unable to load identity from state: {redirData.State} and redirect data: {redirData.ToString()}");
            return new MalarkeyHttpErrorMessageResult($"Unable to load identity from state: {redirData.State} and redirect data: {redirData.ToString()}");
        }
        DebugLog($"Resolved identity: {identity.ToPropertiesString()}");
        MalarkeyProfileAndIdentities? profileAndIdentities = null;
        if(session.ExistingProfileId != null)
        {
            profileAndIdentities = await _profileRepo.AttachIdentityToProfile(identity, session.ExistingProfileId.Value);
            DebugLog($"Attached identity to existing profile: {profileAndIdentities.Profile.ProfileId}");
        }
        else
        {
            profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
            DebugLog($"Created new profile with ID: {profileAndIdentities?.Profile?.ProfileId}");
        }
        if (profileAndIdentities == null)
        {
            _logger.LogError($"Unable to load/create profile and/or identities for identity {identity.ProvidersIdForIdentity} from provider: {identity.IdentityProvider}");
            return new MalarkeyHttpErrorMessageResult($"Unable to load/create profile and/or identities for identity {identity.ProvidersIdForIdentity} from provider: {identity.IdentityProvider}");
        }
        _logger.LogInformation($"Resolved profile: {profileAndIdentities.Profile.ProfileId} and {profileAndIdentities.Identities.Count} identities");
        _events.RegisterIdentificationCompleted(identity);
        var tokenResult = await ResolveTokens(request.HttpContext, profileAndIdentities, session);
        var identityTokens = tokenResult.IdentityTokens
            .Select(_ => _.TokenString)
            .ToList();
        var returnee = new MalarkeyServerAuthenticationForwardHttpResult(session, tokenResult.ProfileTokenString, identityTokens, _logger);
        return returnee;
    }

    private async Task<TokenResult> ResolveTokens(HttpContext context, MalarkeyProfileAndIdentities profileAndIdentities, MalarkeyAuthenticationSession session)
    {
        var usedProvider = session.RequestedIdProvider!.Value;
        var (profileToken, profileTokenString) = await _tokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        var existingIdentityTokens = (await _tokenHandler.ValidateIdentityTokens(context, session.Audience)).Results
            .Select(_ => _ as MalarkeyTokenValidationSuccessResult)
            .Where(_ => _ != null)
            .Select(_ => (Token: (_!.ValidToken as MalarkeyIdentityToken)!, _.TokenString))
            .Select(_ => (_.Token, _.TokenString, _.Token.Identity))
            .Where(_ => _.Identity.IdentityProvider != usedProvider)
            .ToList();
        var existingIdentityTokenIdentityIds = existingIdentityTokens
            .Select(_ => _.Identity.IdentityId)
            .ToHashSet();
        var identitiesForTokenIssue = profileAndIdentities.Identities
        .Where(_ => !existingIdentityTokenIdentityIds.Contains(_.IdentityId))
            .ToList();
        var newlyIssuedIdentityTokens = await _tokenHandler.IssueTokens(identitiesForTokenIssue, session.Audience);
        var tokenForRelevantIdentity = newlyIssuedIdentityTokens
            .First(_ => _.Token.Identity.IdentityProvider == usedProvider);
        await _sessionRepo.UpdateSessionWithTokenInfo(session, profileToken, tokenForRelevantIdentity.Token);
        var allIdentityTokens = existingIdentityTokens
            .Select(_ => (_.Token,_.TokenString))
            .Union(
               newlyIssuedIdentityTokens
                 .Select(_ =>(Token: _.Token, _.TokenString))
            ).ToList();
        var returnee = new TokenResult(profileToken, profileTokenString, allIdentityTokens);
        return returnee;
    }



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
        var returnee = await _sessionRepo.LoadByState(sessionState);
        return returnee;
    }

    private bool IsBlazorRequest => Request.Path.ToString().ToLower().StartsWith("/_blazor");


    private void DebugLog(string str) => 
       _logger.LogInformation(str);


    private record TokenResult(
        MalarkeyProfileToken ProfileToken,
        string ProfileTokenString,
        IReadOnlyCollection<(MalarkeyIdentityToken Token, string TokenString)> IdentityTokens
        );

}
