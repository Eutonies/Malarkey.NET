using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Profile.Model;
public record IdentityProviderIdentityDto(
    string ProviderId,
    IdentityProviderDto Provider,
    string Name,
    string? MiddleNames,
    string? LastName
    );



