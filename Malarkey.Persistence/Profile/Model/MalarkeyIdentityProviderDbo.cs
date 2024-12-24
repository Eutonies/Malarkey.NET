using Malarkey.Domain.Authentication;
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
    public static MalarkeyIdentityProviderDbo ToDbo(this MalarkeyOAuthIdentityProvider prov) => prov switch {
        MalarkeyOAuthIdentityProvider.Microsoft => MalarkeyIdentityProviderDbo.Microsoft,
        MalarkeyOAuthIdentityProvider.Google => MalarkeyIdentityProviderDbo.Google,
        MalarkeyOAuthIdentityProvider.Facebook => MalarkeyIdentityProviderDbo.Facebook,
        MalarkeyOAuthIdentityProvider.Spotify => MalarkeyIdentityProviderDbo.Spotify,
        _ => throw new NotImplementedException()
    };

    public static MalarkeyOAuthIdentityProvider ToDomain(this MalarkeyIdentityProviderDbo prov) => prov switch
    {
        MalarkeyIdentityProviderDbo.Microsoft => MalarkeyOAuthIdentityProvider.Microsoft,
        MalarkeyIdentityProviderDbo.Google => MalarkeyOAuthIdentityProvider.Google,
        MalarkeyIdentityProviderDbo.Facebook => MalarkeyOAuthIdentityProvider.Facebook,
        MalarkeyIdentityProviderDbo.Spotify => MalarkeyOAuthIdentityProvider.Spotify,
        _ => throw new NotImplementedException()
    };
}
