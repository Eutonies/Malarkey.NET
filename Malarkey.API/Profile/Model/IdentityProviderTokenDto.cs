using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Profile.Model;
public record IdentityProviderTokenDto(
    DateTime IssuedAt,
    DateTime Expires,
    string TokenString
    );
