using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;

public sealed record MicrosoftIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string MicrosoftId,
    string PreferredName,
    string Name,
    string? MiddleNames,
    string? LastName
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        MicrosoftId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override MalarkeyIdentityProvider IdentityProvider => MalarkeyIdentityProvider.Microsoft;
}
