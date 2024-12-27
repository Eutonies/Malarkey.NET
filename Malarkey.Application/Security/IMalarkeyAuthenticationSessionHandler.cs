using Malarkey.Domain.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Profile;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
public interface IMalarkeyAuthenticationSessionHandler
{
    Task<MalarkeyAuthenticationSession> InitSession(
        MalarkeyIdentityProvider idProvider, 
        string? forwarder, 
        string audiencePublicKey,
        string[]? scopes);
    Task<MalarkeyAuthenticationSession?> SessionForState(string state);

    Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken, 
        MalarkeyIdentityToken identityToken);

}
