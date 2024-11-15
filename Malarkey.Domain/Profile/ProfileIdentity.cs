using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public abstract record ProfileIdentity
{
    internal ProfileIdentity(
        Guid identityId,
        Guid profileId,
        string providerId,
        string firstName,
        string? middleNames,
        string? lastName
        )
    {
        IdentityId = identityId;
        ProviderId = providerId;
        ProfileId = profileId;
        FirstName = firstName;
        MiddleNames = middleNames;
        LastName = lastName;
    }
    public abstract string IdentityType { get; }

    public string ProviderId { get; private set; }
    public Guid IdentityId { get; private set; }

    public Guid ProfileId { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleNames { get; private set; }
    public string? LastName { get; private set; }
}
