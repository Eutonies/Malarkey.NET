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
    string PreferredName,
    string Name,
    string? MiddleNames,
    string? LastName,
    string? Email
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
