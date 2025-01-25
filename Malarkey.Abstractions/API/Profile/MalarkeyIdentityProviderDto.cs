using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile;
public enum MalarkeyIdentityProviderDto
{
    Microsoft = 1,
    Google = 10,
    Facebook = 20,
    Spotify = 30
}


public static class MalarkeyIdentityProviderDtoExtensions
{
    public static MalarkeyIdentityProvider ToDomain(this MalarkeyIdentityProviderDto identityProviderDto) => identityProviderDto switch {
        MalarkeyIdentityProviderDto.Microsoft => MalarkeyIdentityProvider.Microsoft,
        MalarkeyIdentityProviderDto.Google => MalarkeyIdentityProvider.Google,
        MalarkeyIdentityProviderDto.Facebook => MalarkeyIdentityProvider.Facebook,
        _ => MalarkeyIdentityProvider.Spotify,
    };

    public static MalarkeyIdentityProviderDto ToDto(this MalarkeyIdentityProvider identityProvider) => identityProvider switch
    {
        MalarkeyIdentityProvider.Microsoft => MalarkeyIdentityProviderDto.Microsoft,
        MalarkeyIdentityProvider.Google => MalarkeyIdentityProviderDto.Google,
        MalarkeyIdentityProvider.Facebook => MalarkeyIdentityProviderDto.Facebook,
        _ => MalarkeyIdentityProviderDto.Spotify,
    };

}