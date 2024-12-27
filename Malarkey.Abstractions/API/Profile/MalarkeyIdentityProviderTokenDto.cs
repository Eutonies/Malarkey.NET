using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile;
public record MalarkeyIdentityProviderTokenDto(
    string Token,
    DateTime Issued,
    DateTime Expires
    );
