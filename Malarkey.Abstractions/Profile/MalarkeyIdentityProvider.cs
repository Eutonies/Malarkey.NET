using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;
public enum MalarkeyIdentityProvider
{
    Microsoft = 1,
    Google = 10,
    Facebook = 20,
    Spotify = 30
}

public static class MalarkeyIdentityProviders
{
    public static readonly IReadOnlyCollection<MalarkeyIdentityProvider> AllProviders = [
        MalarkeyIdentityProvider.Microsoft,
        MalarkeyIdentityProvider.Google,
        MalarkeyIdentityProvider.Facebook,
        MalarkeyIdentityProvider.Spotify
        ];
}
