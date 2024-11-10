using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public abstract record ProfileIdentity
{
    internal ProfileIdentity(
        long internalProfileId,
        string firstName,
        string? middleNames,
        string? lastName
        )
    {
        InternalProfileId = internalProfileId;
        FirstName = firstName;
        MiddleNames = middleNames;
        LastName = lastName;
    }

    public long InternalProfileId { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleNames { get; private set; }
    public string? LastName { get; private set; }
}
