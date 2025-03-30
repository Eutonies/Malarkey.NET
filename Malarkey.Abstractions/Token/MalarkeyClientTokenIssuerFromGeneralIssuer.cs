using Malarkey.Abstractions.Profile;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
internal class MalarkeyClientTokenIssuerFromGeneralIssuer : IMalarkeyClientTokenIssuer
{
    private readonly string _ownPublicKey;
    private readonly IMalarkeyTokenIssuer _underlying;
    private readonly IServiceScopeFactory _scopeFactory;

    public MalarkeyClientTokenIssuerFromGeneralIssuer(string ownPublicKey, IMalarkeyTokenIssuer underlying, IServiceScopeFactory scopeFactory)
    {
        _ownPublicKey = ownPublicKey;
        _underlying = underlying;
        _scopeFactory = scopeFactory;
    }

    public IServiceScopeFactory ServiceScopeFactory => _scopeFactory;

    public string CreateTokenString(MalarkeyProfileToken profileToken) => _underlying.CreateTokenString(profileToken, _ownPublicKey);

    public string CreateTokenString(MalarkeyIdentityToken identityToken) => _underlying.CreateTokenString(identityToken, _ownPublicKey);

    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile) => _underlying.IssueToken(profile, _ownPublicKey);

    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(MalarkeyProfileIdentity identity) => _underlying.IssueToken(identity, _ownPublicKey);

    public Task RecallToken(string tokenString) => _underlying.RecallToken(tokenString);

    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<string> tokens) =>
        _underlying.ValidateTokens(tokens.Select(_ => (Token: _, ReceiverPublicKey: _ownPublicKey)));

}
