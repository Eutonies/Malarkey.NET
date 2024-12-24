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
    string? LastName
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
