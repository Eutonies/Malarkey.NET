using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Profile.Model;
public record ProfileDto(
    Guid ProfileId,
    string FirstName,
    string? MiddleNames,
    string LastName,
    IReadOnlyCollection<IdentityProviderIdentityDto> Identities
    );
