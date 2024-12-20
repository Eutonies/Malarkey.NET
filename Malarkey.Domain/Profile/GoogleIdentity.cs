using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public sealed record GoogleIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string GoogleId,
    string Name,
    string? MiddleNames,
    string? LastName
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        GoogleId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override string IdentityType => "google";
}
