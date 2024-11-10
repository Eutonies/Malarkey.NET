using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;

public sealed record MicrosoftIdentity(
    long InternalProfileId,
    string MicrosoftId,
    string PreferredName,
    string Name,
    string? MiddleNames,
    string? LastName
    ) : ProfileIdentity(
        InternalProfileId,
        Name,
        MiddleNames,
        LastName
        );
