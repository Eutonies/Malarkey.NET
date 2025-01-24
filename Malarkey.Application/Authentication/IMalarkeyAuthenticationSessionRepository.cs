using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Profile;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Authentication;
public interface IMalarkeyAuthenticationSessionRepository
{

    Task<MalarkeyAuthenticationSession> RequestInitiateSession(MalarkeyAuthenticationSession session);
    Task<MalarkeyAuthenticationSession> RequestUpdateSession(long sessionId, MalarkeyIdentityProvider identityProvider);

    Task<MalarkeyAuthenticationSession> RequestInitiateIdpSession(long sessionId, MalarkeyAuthenticationIdpSession session);

    Task<MalarkeyAuthenticationSession?> RequestLoadByState(string state);

    Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken,
        MalarkeyIdentityToken identityToken);

    Task<IdentityProviderToken?> Refresh(string accessToken, string audiencePublicKey);


}
