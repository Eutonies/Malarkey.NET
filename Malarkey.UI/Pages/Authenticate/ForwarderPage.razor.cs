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
        $"backward?{nameof(State)}={state.UrlEncoded()}&{nameof(StateVerifier)}={verifier.UrlEncoded()}&{nameof(ProfileId)}={profileId.ToString().UrlEncoded()}";


    [SupplyParameterFromQuery(Name = nameof(State))]
    public string? State { get; set; }

    [SupplyParameterFromQuery(Name = nameof(StateVerifier))]
    public string? StateVerifier { get; set; }

    [SupplyParameterFromQuery(Name= nameof(ProfileId))]
    public string? ProfileId { get; set; }
    private Guid ProfileGuid => Guid.Parse(ProfileId!);


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

    [Inject]
    public ILogger<ForwarderPage> Logger { get; set; }


    private string _targetUrl = "";
    private string _profileToken = "";
    private IReadOnlyCollection<string> _identityTokens = [];
    private string _returnState = "";



    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(StateVerifier != null) {
            var stateForVerifier = Forwarder.StateFor(StateVerifier);
            if(stateForVerifier != State) 
            {
                Logger.LogInformation($"State and state verifier don't be matching boss");
                Logger.LogInformation($"  State: {State}");
                Logger.LogInformation($"  Verifier: {stateForVerifier}");
            }
            else
            {
                Logger.LogInformation($"State and verifier matched boss!");
                var session = await Repo.LoadByState(State);
                if(session== null) 
                {
                    Logger.LogInformation($"Unable to load session for state: {State} boss");
                }
                else
                {
                    Logger.LogInformation($"Dun loaded the session for state: {State} boss!");
                    _targetUrl = session.SendTo;
                    Logger.LogInformation($"Be sendin' dem to: {_targetUrl} boss!");
                    await ResolveParameters(session);
                    await JS.InvokeVoidAsync("submitData");
                }
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
        _identityTokens = allIdentityTokens.ToList();
        if (session.EncryptState)
        {
            Logger.LogInformation($"Will encrypt state using audience certificate: {session.Audience}");
            var receiverCertBytes = Convert.FromBase64String(session.Audience);
            Logger.LogInformation($"  That's {receiverCertBytes.Length} bytes worth of certificate");
            var recieverCertificate = X509CertificateLoader.LoadCertificate(receiverCertBytes);
            Logger.LogInformation($"  Loaded certificate with friendly name: {recieverCertificate.FriendlyName}");
            var encryptedStateBytes = recieverCertificate.PublicKey.GetRSAPublicKey()!.Encrypt(UTF8Encoding.UTF8.GetBytes(State), MalarkeyConstants.RSAPadding);
            var encryptedState = UTF8Encoding.UTF8.GetString(encryptedStateBytes);
            _returnState = encryptedState;
            Logger.LogInformation($"  Return state will be: {_returnState}");
        }
        else
            _returnState = State;

    }

}
