using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using Malarkey.Application.Configuration;
using Malarkey.Security.Formats;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Malarkey.Application.Util;

namespace Malarkey.Security;
internal class MalarkeyTokenHandler : IMalarkeyTokenHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MalarkeyApplicationConfiguration _securityConfiguration;
    private readonly X509Certificate2 _certificate;
    private readonly RsaSecurityKey _rsaPublicKey;
    private readonly RsaSecurityKey _rsaPrivateKey;
    private readonly JsonWebTokenHandler _tokenHandler;
    private readonly SigningCredentials _credentials;

    public IServiceScopeFactory ServiceScopeFactory => _scopeFactory;

    public string PublicKey => _rsaPublicKey.Rsa.ExportRSAPublicKeyPem();

    public MalarkeyTokenHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();
        _securityConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<MalarkeyApplicationConfiguration>>().Value;
        _certificate = _securityConfiguration.SigningCertificate.AsCertificate;
        _rsaPublicKey = new RsaSecurityKey(_certificate.GetRSAPublicKey());
        _rsaPrivateKey = new RsaSecurityKey(_certificate.GetRSAPrivateKey());
        _tokenHandler = new JsonWebTokenHandler();
        _credentials = new SigningCredentials(_rsaPrivateKey, SecurityAlgorithms.RsaSsaPssSha256);
    }
    public async Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey)
    {
        receiverPublicKey = receiverPublicKey.CleanCertificate();
        await Task.CompletedTask;
        var token = new MalarkeyProfileToken(
            TokenId: Guid.NewGuid(),
            IssuedTo: receiverPublicKey,
            IssuedAt: MalarkeySecurityConstants.Now,
            ValidUntil: MalarkeySecurityConstants.Now + MalarkeySecurityConstants.TokenLifeTime,
            Profile: profile
            );
        var payload = profile.ToPayloadTso(receiverPublicKey, expiresAt: token.ValidUntil, token.TokenId);
        var tokenString = CreateTokenString(token, payload);
        return (token,  tokenString);
    }


    public async Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(ProfileIdentity identity, string receiverPublicKey)
    {
        receiverPublicKey = receiverPublicKey.CleanCertificate();
        await Task.CompletedTask;
        var token = new MalarkeyIdentityToken(
            TokenId: Guid.NewGuid(),
            IssuedTo: receiverPublicKey,
            IssuedAt: MalarkeySecurityConstants.Now,
            ValidUntil: MalarkeySecurityConstants.Now + MalarkeySecurityConstants.TokenLifeTime,
        Identity: identity
            );
        var payload = identity.ToPayloadTso(receiverPublicKey, expiresAt: token.ValidUntil, token.TokenId);
        var tokenString = CreateTokenString(token, payload);
        return (token, tokenString);
    }


    private string CreateTokenString(MalarkeyToken token, MalarkeyTokenPayloadTso payload)
    {
            var header = token.ToHeaderTso();
            var tokenString = _tokenHandler.CreateToken(payload.ToTokenDescriptor(header, _credentials));
            return tokenString;
    }


    public Task RecallToken(string tokenString)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<(string Token, string ReceiverPublicKey)> tokens)
    {
        var tasks = tokens
            .Select(async tokPar => await CheckToken(tokPar.Token, tokPar.ReceiverPublicKey))
            .ToList();
        var returnee = await Task.WhenAll(tasks);
        return returnee;

    }

    private async Task<MalarkeyTokenValidationResult> CheckToken(string token, string receiver)
    {
        try
        {
            receiver= receiver.CleanCertificate();
            var result = await _tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidIssuer = MalarkeySecurityConstants.TokenIssuer,
                ValidAudience = receiver,
                IssuerSigningKey = _rsaPublicKey
            });
            if (result.IsValid)
            {
                var readToken = _tokenHandler.ReadToken(token);
                var tokenTso = readToken.ToMalarkeyTokenTso();
                var returnToken = tokenTso.ToDomain();
                return new MalarkeyTokenValidationSuccessResult(token, returnToken);
            }
            return new MalarkeyTokenValidationExceptionResult(token, result.Exception);
        }
        catch (Exception ex)
        {
            return new MalarkeyTokenValidationExceptionResult(token, ex);
        }

    }



}

internal static class MalarkeyTokenHandlerExtensions
{

    internal static MalarkeyTokenTso ToMalarkeyTokenTso(this SecurityToken tok) => tok switch
    {
        JsonWebToken jwt => jwt.EncodedToken.DeserializeToMalarkeyToken(),
        _ => throw new Exception("")
    };
        
    internal static SecurityTokenDescriptor ToTokenDescriptor(this MalarkeyTokenPayloadTso payload, MalarkeyTokenHeaderTso header, SigningCredentials signingCredentials) => new SecurityTokenDescriptor
    {
        Issuer = payload.iss,
        Audience = payload.aud,
        IssuedAt = payload.iat.ParseJwtTime(),
        NotBefore = payload.iat.ParseJwtTime(),
        Expires = payload.exp.ParseJwtTime(),
        Subject = new ClaimsIdentity(payload.ExtractAdditionalClaims().Prepend(new Claim("sub", payload.sub))),
        SigningCredentials = signingCredentials,
        AdditionalHeaderClaims = header.ExtractAdditionalHeaderClaims()
    };


    internal static IReadOnlyCollection<Claim> ExtractAdditionalClaims(this MalarkeyTokenPayloadTso payload) => new List<(string, string?)>
    {
        (nameof(payload.identid), payload.identid),
        (nameof(payload.identtyp), payload.identtyp),
        (nameof(payload.jti), payload.jti),
        (nameof(payload.name), payload.name),
        (nameof(payload.midnames), payload.midnames),
        (nameof(payload.lastname), payload.lastname),
        (nameof(payload.prefname), payload.prefname)
    }.Where(_ => _.Item2 != null)
        .Select(_ => new Claim(_.Item1, _.Item2!))
        .ToList();

    internal static IDictionary<string, object> ExtractAdditionalHeaderClaims(this MalarkeyTokenHeaderTso header) => new List<(string, object?)>
    {
        (nameof(header.toktyp), header.toktyp)
    }.Where(_ => _.Item2 != null)
        .ToDictionary(_ => _.Item1, _ => _.Item2!);


}

