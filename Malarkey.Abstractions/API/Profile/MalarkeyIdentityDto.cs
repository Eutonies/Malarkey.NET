using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile;
public record MalarkeyIdentityDto(
    Guid IdentityId,
    Guid ProfileId,
    MalarkeyIdentityProviderDto IdentityProvider,
    string SpotifyId,
    string Name,
    string? MiddleNames,
    string? LastName,
    string? Email,
    MalarkeyIdentityProviderTokenDto? IdProviderToken
    );
