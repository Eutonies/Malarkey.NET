using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public record ProfileIdentity(
    long InternalProfileId,
    Guid ProfileId,
    string FirstName,
    string? MiddleNames,
    string LastName
    );
