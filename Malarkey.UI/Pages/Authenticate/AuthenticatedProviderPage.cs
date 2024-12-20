using Malarkey.Application.Configuration;
using Malarkey.Application.Profile;
using Malarkey.Application.Security;
using Malarkey.Application.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Malarkey.UI.Pages.Authenticate;

public class AuthenticatedProviderPage : ComponentBase
{

    [Inject]
    public IHttpContextAccessor ContextAccessor { get; set; }

    [Inject]
    public IMalarkeyTokenHandler TokenHandler { get; set; }

    [Inject]
    public IProfileService ProfileService { get; set; }

    [Inject]
    public IOptions<MalarkeyApplicationConfiguration> ApplicationConfiguration { get; set; }

    private string? _publicKey;

    protected override async Task OnInitializedAsync()
    {
        _publicKey ??= ApplicationConfiguration.Value.HostingCertificate.AsCertificate.PublicKey.GetRSAPublicKey()!.ExportRSAPublicKeyPem().CleanCertificate();

    }



    protected async Task ExtractIdentities()
    {
        var context = ContextAccessor.HttpContext;
        foreach(var ident in context!.User.Identities)
        {
        }
    }



}
