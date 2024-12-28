using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile.Persistence;
public enum MalarkeyIdentityProviderDbo
{
    Microsoft = 10,
    Google = 20,
    Facebook = 30,
    Spotify = 40
}


public static class MalarkeyIdentityProviderDboExtensions
{
    public static MalarkeyIdentityProviderDbo ToDbo(this MalarkeyIdentityProvider prov) => prov switch {
        MalarkeyIdentityProvider.Microsoft => MalarkeyIdentityProviderDbo.Microsoft,
        MalarkeyIdentityProvider.Google => MalarkeyIdentityProviderDbo.Google,
        MalarkeyIdentityProvider.Facebook => MalarkeyIdentityProviderDbo.Facebook,
        MalarkeyIdentityProvider.Spotify => MalarkeyIdentityProviderDbo.Spotify,
        _ => throw new NotImplementedException()
    };

    public static MalarkeyIdentityProvider ToDomain(this MalarkeyIdentityProviderDbo prov) => prov switch
    {
        MalarkeyIdentityProviderDbo.Microsoft => MalarkeyIdentityProvider.Microsoft,
        MalarkeyIdentityProviderDbo.Google => MalarkeyIdentityProvider.Google,
        MalarkeyIdentityProviderDbo.Facebook => MalarkeyIdentityProvider.Facebook,
        MalarkeyIdentityProviderDbo.Spotify => MalarkeyIdentityProvider.Spotify,
        _ => throw new NotImplementedException()
    };
}
