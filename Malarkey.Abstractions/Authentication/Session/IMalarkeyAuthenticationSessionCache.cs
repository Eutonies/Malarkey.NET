using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Profile;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public interface IMalarkeyAuthenticationSessionCache
{
    Task<MalarkeyAuthenticationSession> InitiateSession(MalarkeyAuthenticationSession session);
    Task<MalarkeyAuthenticationSession> UpdateSession(long sessionId, MalarkeyIdentityProvider identityProvider);
    Task<MalarkeyAuthenticationSession> InitiateIdpSession(long sessionId, MalarkeyAuthenticationIdpSession session);
    Task<MalarkeyAuthenticationSession?> LoadByState(string state);
    Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken,
        MalarkeyIdentityToken identityToken);

    Task<MalarkeyRefreshTokenData?> LoadRefreshTokenForAccessToken(string accessToken, string subject);
    Task<IdentityProviderToken> UpdateIdentityProviderToken(IdentityProviderToken token);

}
