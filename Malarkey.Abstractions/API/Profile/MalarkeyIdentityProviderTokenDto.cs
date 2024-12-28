using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile;
public record MalarkeyIdentityProviderTokenDto(
    string Token,
    DateTime Issued,
    DateTime Expires,
    string[] Scopes
    )
{
    public IdentityProviderToken ToDomain() => new IdentityProviderToken(
        Token: Token,
        Issued: Issued,
        Expires: Expires,
        RefreshToken: null,
        Scopes: Scopes
        );
}

public static class MalarkeyIdentityProviderTokenDtoExtensions
{
    public static MalarkeyIdentityProviderTokenDto ToDto(this IdentityProviderToken token) => new MalarkeyIdentityProviderTokenDto(
        Token: token.Token,
        Issued: token.Issued,
        Expires: token.Expires,
        Scopes: token.Scopes
    );
}