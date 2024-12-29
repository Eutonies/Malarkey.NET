using Malarkey.UI.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Malarkey.UI.Pages;


public partial class App 
{
    [Inject]
    public IOptions<MalarkeyUIConfiguration> Config { get; set; }


    private string BasePath => string.IsNullOrWhiteSpace(Config.Value.HostingBasePath) ? "/" : $"/{Config.Value.HostingBasePath}/";

}