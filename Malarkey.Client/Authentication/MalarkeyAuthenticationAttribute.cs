using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MalarkeyAuthenticationAttribute : Attribute
{

    public string? Scopes { get; set; }

    public MalarkeyIdentityProvider? IdentityProvider { get; set; }

}
