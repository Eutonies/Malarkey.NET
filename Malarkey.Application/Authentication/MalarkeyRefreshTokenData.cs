using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Authentication;
public record MalarkeyRefreshTokenData(
    string RefreshToken,
    Guid IdentityId,
    MalarkeyIdentityProvider IdentityProvider
    );
