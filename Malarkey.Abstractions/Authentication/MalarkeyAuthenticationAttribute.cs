using Malarkey.Abstractions.Profile;

namespace Malarkey.Abstractions.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MalarkeyAuthenticationAttribute : Attribute
{

    public string? Scopes { get; set; }

    public MalarkeyIdentityProvider? IdentityProvider { get; set; }

}
