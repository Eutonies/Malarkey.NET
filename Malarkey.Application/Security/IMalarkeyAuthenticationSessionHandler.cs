using Malarkey.Abstractions.Authentication;
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
        string[]? scopes,
        string? forwarderState,
        Guid? existingProfileId
        );
    Task<MalarkeyAuthenticationSession?> SessionForState(string state);

    Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken, 
        MalarkeyIdentityToken identityToken);

    Task<IdentityProviderToken?> Refresh(string accessToken, string audiencePublicKey);


}
