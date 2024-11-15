using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;

public sealed record MicrosoftIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string MicrosoftId,
    string PreferredName,
    string Name,
    string? MiddleNames,
    string? LastName
    ) : ProfileIdentity(
        IdentityId,
        ProfileId,
        MicrosoftId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override string IdentityType => "microsoft";
}
