using Malarkey.Domain.Authentication;
using Malarkey.Domain.Token;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
public interface IMalarkeyAuthenticationSessionHandler
{
    Task<MalarkeyAuthenticationSession> InitSession(MalarkeyOAuthIdentityProvider idProvider, string? forwarder, string audiencePublicKey);
    Task<MalarkeyAuthenticationSession?> SessionForState(string state);

    Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken, 
        MalarkeyIdentityToken identityToken);

}
