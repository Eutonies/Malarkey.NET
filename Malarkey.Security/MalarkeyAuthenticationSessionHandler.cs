using Malarkey.Application.Security;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Token;
using Malarkey.Domain.Util;
using Malarkey.Security.Persistence;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Malarkey.Security;
internal class MalarkeyAuthenticationSessionHandler : IMalarkeyAuthenticationSessionHandler
{
    private static readonly char[] CodeVerifierAllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~"
        .ToCharArray();

    private readonly IMalarkeySessionRepository _repo;

    public MalarkeyAuthenticationSessionHandler(IMalarkeySessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<MalarkeyAuthenticationSession> InitSession(MalarkeyOAuthIdentityProvider idProvider, string? forwarder, string audiencePublicKey)
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
            audience: audiencePublicKey
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
}
