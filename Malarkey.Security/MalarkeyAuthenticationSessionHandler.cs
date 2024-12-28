using Malarkey.Application.Security;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Malarkey.Security.Persistence;
using System.Security.Cryptography;
using System.Text;
using Malarkey.Abstractions.Profile;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.Security;
internal class MalarkeyAuthenticationSessionHandler : IMalarkeyAuthenticationSessionHandler
{
    private static readonly char[] CodeVerifierAllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~"
        .ToCharArray();

    private readonly IMalarkeySessionRepository _repo;
    private readonly IServiceScopeFactory _scopeFactory;

    public MalarkeyAuthenticationSessionHandler(IMalarkeySessionRepository repo, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _repo = repo;
    }

    public async Task<MalarkeyAuthenticationSession> InitSession(
        MalarkeyIdentityProvider idProvider, 
        string? forwarder, 
        string audiencePublicKey,
        string[]? scopes)
    {
        var nonce = GenerateNonce();
        var (verifier, challenge) = GenerateChallengeAndVerifier();
        var session = await _repo.InitNewSession(
            idProvider: idProvider,
            nonce: nonce,
            forwarder: forwarder,
            codeChallenge: challenge,
            codeVerifier: verifier,
            initTime: DateTime.Now,
            audience: audiencePublicKey,
            scopes: scopes
        );
        return session;
    }

    public Task<MalarkeyAuthenticationSession?> SessionForState(string state) => _repo.SessionFor(state);


    private static string GenerateNonce()
    {
        using var random = RandomNumberGenerator.Create();
        var randomBytes = new byte[32];
        random.GetBytes(randomBytes);
        var returnee = Convert.ToBase64String(randomBytes);
        return returnee;
    }

    private static (string Verifier, string Challenge) GenerateChallengeAndVerifier()
    {
        using var random = RandomNumberGenerator.Create();
        var randomBytes = new byte[43];
        random.GetBytes(randomBytes);
        randomBytes = randomBytes
            .Select(_ => (byte)( _ % CodeVerifierAllowedChars.Length))
            .ToArray();
        var verifier = randomBytes
            .Select(byt => CodeVerifierAllowedChars[byt])
            .MakeString("");
        var verifierBytes = UTF8Encoding.UTF8.GetBytes(verifier);
        var challengeBytes = SHA256.HashData(verifierBytes);
        var challenge = Convert.ToBase64String(challengeBytes).Substring(0,43);
        return (verifier, challenge);
    }

    public async Task<MalarkeyAuthenticationSession> UpdateSessionWithTokenInfo(
        MalarkeyAuthenticationSession session,
        MalarkeyProfileToken profileToken, 
        MalarkeyIdentityToken identityToken) => 
            (await _repo.UpdateWithAuthenticationInfo(session.State, DateTime.Now, profileToken.TokenId, identityToken.TokenId))!;

    public async Task<IdentityProviderToken?> Refresh(string accessToken, string audiencePublicKey)
    {
        var loadedInfo = await _repo.LoadRefreshTokenForAccessToken(accessToken, audiencePublicKey);
        if (loadedInfo == null)
            return null;
        using var scope = _scopeFactory.CreateScope();
        var refresher = scope.ServiceProvider.GetKeyedService<IMalarkeyIdentityProviderTokenRefresher>(loadedInfo.Provider);
        if (refresher == null)
            return null;
        var refreshed = await refresher.Refresh(loadedInfo.Token);
        if (refreshed == null) 
            return null;
        await _repo.SaveRefreshedToken(refreshed, loadedInfo.IdentityId);
        return refreshed;

    }
}
