using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public sealed record SpotifyIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string SpotifyId,
    string Name,
    string? MiddleNames,
    string? LastName,
    string? Email,
    IdentityProviderToken? AccessToken
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        SpotifyId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override string IdentityType => "spotify";
}
