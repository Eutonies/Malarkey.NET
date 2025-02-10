using Azure.Core;
using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Malarkey.Application.Authentication;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Application.Security;
using Malarkey.Integration.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace Malarkey.UI.Pages.Authenticate;

public partial class ForwarderPage
{
    public static string UrlFor(string state, string verifier, Guid profileId) =>
        $"backward?state={state.UrlEncoded()}&stateVerifier={verifier.UrlEncoded()}&profileId={profileId.ToString().UrlEncoded()}";


    [SupplyParameterFromQuery]
    public string State { get; set; }

    [SupplyParameterFromQuery]
    public string StateVerifier { get; set; }

    [SupplyParameterFromQuery]
    public string ProfileId { get; set; }
    private Guid ProfileGuid => Guid.Parse(ProfileId);


    [Inject]
    public IMalarkeyServerAuthenticationForwarder Forwarder { get; set; }

    [Inject]
    public IMalarkeyAuthenticationSessionRepository Repo { get; set; }
    [Inject]
    public IMalarkeyProfileRepository ProfileRepo { get; set; }

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }


    [Inject]
    public IMalarkeyTokenHandler TokenHandler { get; set; }

    [Inject]
    public IJSRuntime JS { get; set; }


    private string _targetUrl = "";
    private string _profileToken = "";
    private IReadOnlyCollection<string> _identityTokens = [];
    private string _returnState = "";



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var stateForVerifier = Forwarder.StateFor(StateVerifier);
        if(stateForVerifier == State)
        {
            var session = await Repo.LoadByState(State);
            if(session != null)
            {
                _targetUrl = session.SendTo;
                await ResolveParameters(session);
                await JS.InvokeVoidAsync("submitData");
            }
        }
    }



    private async Task ResolveParameters(MalarkeyAuthenticationSession session)
    {
        var profileAndIdentities = await ProfileRepo.LoadProfileAndIdentities(ProfileGuid);
        if (profileAndIdentities == null)
            return;
        var context = ContextAccessor.HttpContext;
        if(context == null) 
            return;
        var usedProvider = session.RequestedIdProvider!.Value;
        var (profileToken, profileTokenString) = await TokenHandler.IssueToken(profileAndIdentities.Profile, session.Audience);
        _profileToken = profileTokenString;
        var existingIdentityTokens = (await TokenHandler.ValidateIdentityTokens(context, session.Audience)).Results
            .Select(_ => _ as MalarkeyTokenValidationSuccessResult)
            .Where(_ => _ != null)
            .Select(_ => (Identity: (_!.ValidToken as MalarkeyIdentityToken)!.Identity, TokenString: _.TokenString))
            .Where(_ => _.Identity.IdentityProvider != usedProvider)
            .ToList();
        var existingIdentityTokenIdentityIds = existingIdentityTokens
            .Select(_ => _.Identity.IdentityId)
            .ToHashSet();
        var identitiesForTokenIssue = profileAndIdentities.Identities
        .Where(_ => !existingIdentityTokenIdentityIds.Contains(_.IdentityId))
            .ToList();
        var newlyIssuedIdentityTokens = await TokenHandler.IssueTokens(identitiesForTokenIssue, session.Audience);
        var tokenForRelevantIdentity = newlyIssuedIdentityTokens
            .First(_ => _.Token.Identity.IdentityProvider == usedProvider);
        await Repo.UpdateSessionWithTokenInfo(session, profileToken, tokenForRelevantIdentity.Token);
        var allIdentityTokens = existingIdentityTokens
            .Select(_ => _.TokenString)
            .Union(
               newlyIssuedIdentityTokens
                 .Select(_ => _.TokenString)
            ).ToList();
        context.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName, profileTokenString);
        for(var i = 0; i < allIdentityTokens.Count; i++)
            context.Response.Cookies.Append(MalarkeyConstants.Authentication.IdentityCookieName(i), allIdentityTokens[i]);
        _identityTokens = allIdentityTokens.ToList();
        if (session.EncryptState)
        {
            var recieverCertificate = X509Certificate2.CreateFromPem(session.Audience);
            var encryptedStateBytes = recieverCertificate.PublicKey.GetRSAPublicKey()!.Encrypt(UTF8Encoding.UTF8.GetBytes(State), MalarkeyConstants.RSAPadding);
            var encryptedState = UTF8Encoding.UTF8.GetString(encryptedStateBytes);
            _returnState = encryptedState;
        }
        else
            _returnState = State;

    }

}
