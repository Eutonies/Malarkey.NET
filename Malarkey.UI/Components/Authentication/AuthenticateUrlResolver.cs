using Malarkey.Integration.Authentication;
using Malarkey.UI.Configuration;

namespace Malarkey.UI.Components.Authentication;

public class AuthenticateUrlResolver : IAuthenticationUrlResolver
{
    public string AuthenticateUrl => MalarkeyUIConfiguration.LoginUrl;
}
