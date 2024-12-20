using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public sealed record FacebookIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string FacebookId,
    string Name,
    string? MiddleNames,
    string? LastName
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        FacebookId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override string IdentityType => "facebook";
}
