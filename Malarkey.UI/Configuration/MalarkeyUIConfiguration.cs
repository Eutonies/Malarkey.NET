using Malarkey.UI.Pages.Authenticate;
using Malarkey.UI.Util;
using Microsoft.AspNetCore.Mvc;

namespace Malarkey.UI.Configuration;

public class MalarkeyUIConfiguration 
{

    public const string ConfigurationElementName = "UI";
    
    public string? HostingBasePath { get; set; }

    private static string? _loginUrl;
    public static string LoginUrl => _loginUrl ??= (typeof(AuthenticatePage).RouteOf() ?? "");

}