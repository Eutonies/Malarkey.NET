using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public interface IMalarkeyIdentityProviderTokenRefresher
{
    Task<IdentityProviderToken?> Refresh(string refreshToken, string audiencePublicKey);

}
