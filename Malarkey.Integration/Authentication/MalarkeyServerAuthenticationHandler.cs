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
using Malarkey.Application.Authentication;
using Malarkey.Abstractions.Token;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>, IMalarkeyServerAuthenticationCallbackHandler
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionRepository _sessionRepo;
    private readonly IReadOnlyDictionary<MalarkeyIdentityProvider, IMalarkeyOAuthFlowHandler> _flowHandlers;
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly MalarkeyIntegrationConfiguration _intConf;
    private readonly ILogger<MalarkeyServerAuthenticationHandler> _logger;
    private readonly IAuthenticationUrlResolver _urlResolver;
    private readonly string _malarkeyTokenReceiver;
    private readonly IMalarkeyServerAuthenticationEventHandler _events;


    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyServerAuthenticationHandlerOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IMalarkeyTokenHandler tokenHandler,
        IMalarkeyAuthenticationSessionRepository sessionRepo,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IMalarkeyProfileRepository profileRepo,
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        ILogger<MalarkeyServerAuthenticationHandler> logger,
        IAuthenticationUrlResolver urlResolver,
        IMalarkeyServerAuthenticationEventHandler events) : base(options, loggerFactory, encoder)
    {
        _events = events;
        _urlResolver = urlResolver;
        _logger = logger;
        _intConf = intConf.Value;
        _tokenHandler = tokenHandler;
        _sessionRepo = sessionRepo;
        _flowHandlers = flowHandlers
            .ToDictionarySafe(_ => _.HandlerFor);
        _profileRepo = profileRepo;
        _malarkeyTokenReceiver = intConf.Value.PublicKey.CleanCertificate();
    }

    private string TokenReceiver => (Context?.Request?.Headers?.TryGetValue(MalarkeyConstants.Authentication.AudienceHeaderName, out var audience) ?? false) ?
        audience.ToString() :
        _malarkeyTokenReceiver;


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var profileAndIdentities = await _tokenHandler.ExtractProfileAndIdentities(Context, TokenReceiver);
        if(profileAndIdentities == null)
            return AuthenticateResult.Fail("No profile found in cookies");
        var authSession = await LoadSession();
        if(authSession != null)
        {
            if (authSession.AlwaysChallenge)
                return AuthenticateResult.Fail("Asked to always challenge");
            if(authSession.RequestedIdProvider != null && !profileAndIdentities.Identities.Any(_ => _.IdentityProvider == authSession.RequestedIdProvider))
                return AuthenticateResult.Fail($"No identity token for: {authSession.RequestedIdProvider} found");
        }
        var (prof, idents, _) = profileAndIdentities;
        var returnee = AuthenticateResult.Success(new AuthenticationTicket(
                principal: prof.ToClaimsPrincipal(idents),
                MalarkeyConstants.MalarkeyAuthenticationScheme
                ));
        return returnee;

    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authSession = await LoadSession();
        if(authSession == null)
        {
            var audience = ExtractPublicKeyOfReceiver();
            authSession = Request.ResolveSession(audience);
            authSession = await _sessionRepo.RequestInitiateSession(authSession);
        }
        var idp = authSession!.RequestedIdProvider;
        if(idp == null)
        {
            idp = Request.ResolveSession("").RequestedIdProvider;
            if (idp != null)
                await _sessionRepo.RequestUpdateSession(authSession.SessionId, idp.Value);
        }
        if (idp == null || !_flowHandlers.TryGetValue(idp.Value, out var flowHandler))
            await CompleteInError("Failed to resolve desired identity provider");
        else
        {
            var idpSession = flowHandler.PopulateIdpSession(authSession);
            authSession = await _sessionRepo.RequestInitiateIdpSession(authSession.SessionId, idpSession);
            var redirectUrl = flowHandler.ProduceAuthorizationUrl(authSession, idpSession);
            Context.Response.StatusCode = 302;
            Context.Response.Redirect(redirectUrl);
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
        var session = await _sessionRepo.RequestLoadByState(redirData.State);
        if(session == null)
        {
            _logger.LogError($"Unable to load session from state: {redirData.State}");
            return await ReturnError($"Could not find session for state={redirData.State}");
        }
        if(session.RequestedIdProvider == null || !_flowHandlers.TryGetValue(session.RequestedIdProvider.Value, out var flowHandler))
        {
            _logger.LogError($"Unable to determine flow handler on callback from state: {redirData.State}");
            return await ReturnError($"Could not find flow handler on callback for state={redirData.State}");
        }
        var identity = await flowHandler.ResolveIdentity(session, redirData);
        if (identity == null)
        {
            _logger.LogError($"Unable to load identity from state: {redirData.State} and redirect data: {redirData.ToString()}");
            return await ReturnError($"Could not resolve identity by {session.RequestedIdProvider} for callback request with URL={request.GetDisplayUrl()}");
        }
        MalarkeyProfileAndIdentities? profileAndIdentities = null;
        if(session.ExistingProfileId != null)
        {
            profileAndIdentities = await _profileRepo.AttachIdentityToProfile(identity, session.ExistingProfileId.Value);
        }
        else
        {
            profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
        }
        if (profileAndIdentities == null)
        {
            _logger.LogError($"Unable to load/create profile and/or identities for identity {identity.ProvidersIdForIdentity} from provider: {identity.IdentityProvider}");
            return await ReturnError($"Could not resolve profile from identity provider ID: {identity.ProvidersIdForIdentity} by {session.RequestedIdProvider}");
        }
        _logger.LogInformation($"Resolved profile: {profileAndIdentities.Profile.ProfileId} and {profileAndIdentities.Identities.Count} identities");
        identity = profileAndIdentities.Identities
            .First(_ => _.IdentityProvider == identity.IdentityProvider && _.ProviderId == identity.ProviderId);
        var (profileToken, profileTokenString) = await _tokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        var existingIdentityTokens = (await _tokenHandler.ValidateIdentityTokens(Context, session.Audience)).Results
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
        var newlyIssuedIdentityTokens = await _tokenHandler.IssueTokens(identitiesForTokenIssue, session.Audience);
        var tokenForRelevantIdentity = newlyIssuedIdentityTokens
            .First(_ => _.Token.Identity.IdentityId == identity.IdentityId);
        await _sessionRepo.UpdateSessionWithTokenInfo(session, profileToken, tokenForRelevantIdentity.Token);
        var allIdentityTokens = existingIdentityTokensToCarry
            .Select(_ => _.TokenString)
            .Union(
               newlyIssuedIdentityTokens
                 .Select(_ => _.TokenString)
            ).ToList();
        _events.RegisterIdentificationCompleted(identity);

        var returnee = new MalarkeyAuthenticationSuccessHttpResult(
            Session: session,
            ProfileToken: profileTokenString,
            IdentityTokens: allIdentityTokens,
            Logger: _logger
            );
        return returnee;
    }



    private Task<BadRequest<string>> ReturnError(string errorMessage) => Task.FromResult(
        TypedResults.BadRequest(errorMessage)
        );

    private string ExtractPublicKeyOfReceiver()
    {
        if (!Request.Headers.TryGetValue(MalarkeyConstants.Authentication.AudienceHeaderName, out var pubKey))
            return _intConf.PublicKey.CleanCertificate();
        return pubKey.ToString();
    }

    private async Task<MalarkeyAuthenticationSession?> LoadSession()
    {
        var sessionState = Request.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.SessionStateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (sessionState == null)
            return null;
        var returnee = await _sessionRepo.RequestLoadByState(sessionState);
        return returnee;
    }



    private async Task CompleteInError(string errorMessage)
    {
        Context.Response.StatusCode = 403;
        await Context.Response.WriteAsync( errorMessage );
    }




}
