using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Domain.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security.Persistence;
public interface IMalarkeySessionRepository
{
    Task<MalarkeyAuthenticationSession> InitNewSession(
        MalarkeyIdentityProvider idProvider, 
        string nonce, 
        string? forwarder, 
        string codeChallenge, 
        string codeVerifier, 
        DateTime initTime,
        string audience,
        string[]? scopes);
    Task<MalarkeyAuthenticationSession?> SessionFor(string state);
    Task<MalarkeyAuthenticationSession?> UpdateWithAuthenticationInfo(string state, DateTime authenticatedTime, Guid profileTokenId, Guid identityTokenId);

    public Task<RefreshTokenLoadData?> LoadRefreshTokenForAccessToken(string accessToken, string clientCertificate);
    public Task SaveRefreshedToken(IdentityProviderToken token, Guid identityId);


    public record RefreshTokenLoadData(
        string Token,
        Guid IdentityId,
        MalarkeyIdentityProvider Provider
        );

}
