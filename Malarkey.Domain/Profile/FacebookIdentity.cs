using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public record FacebookIdentity(
    long InternalProfileId,
    Guid FacebookId,
    string Name,
    string? MiddleNames,
    string? LastName
    );
