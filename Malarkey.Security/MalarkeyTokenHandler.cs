using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using Malarkey.Security.Configuration;
using Malarkey.Security.Formats;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
internal class MalarkeyTokenHandler : IMalarkeyTokenHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SecurityConfiguration _securityConfiguration;
    private readonly X509Certificate2 _certificate;

    public MalarkeyTokenHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();
        _securityConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<SecurityConfiguration>>().Value;
        _certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
            fileName: _securityConfiguration.TokenCertificate,
            password: _securityConfiguration.TokenCertificatePassword
            );
    }
    public async Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey)
    {
        await Task.CompletedTask;
        using var scope = _scopeFactory.CreateScope();
        var token = new MalarkeyProfileToken(
            TokenId: Guid.NewGuid(),
            IssuedTo: receiverPublicKey,
            IssuedAt: DateTime.Now,
            ValidUntil: DateTime.Now + MalarkeySecurityConstants.TokenLifeTime,
            Profile: profile
            );
        var header = token.ToHeaderTso();
        var payload = profile.ToPayloadTso(receiverPublicKey, expiresAt: token.ValidUntil, token.TokenId);
        var headerString = header.Serialize();
        var payloadString = payload.Serialize();
        var toSign = $"{headerString}.{payloadString}";
        var signature = "";
        var tokenTso = new MalarkeyTokenTso(header, payload, signature);
        var tokenString = tokenTso.ToString();
        return (token,  tokenString);
    }


    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(ProfileIdentity identity, string receiverPublicKey)
    {
        throw new NotImplementedException();
    }

    public Task RecallToken(string tokenString)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<TokenValidationResult>> ValidateTokens(IEnumerable<string> tokens)
    {
        throw new NotImplementedException();
    }





}
