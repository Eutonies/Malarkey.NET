using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model;
internal class IdentityProviderTokenDbo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long IdProviderTokenId { get; set; }
    public Guid IdentityId { get; set; }
    public string TokenString { get; set; }
    public DateTime Issued { get; set; }
    public DateTime Expires { get; set; }
    public string? RefreshToken { get; set; }
    public string Scopes { get; set; }

    public IdentityProviderToken ToDomain() => new IdentityProviderToken(
        Token: TokenString,
        Issued: Issued,
        Expires: Expires,
        RefreshToken: RefreshToken,
        Scopes: Scopes.Split(" ")
        );

}


internal static class IdentityProviderTokenDboExtensions
{
    internal static IdentityProviderTokenDbo ToDbo(this IdentityProviderToken token, Guid identityId) => new IdentityProviderTokenDbo
    {
        IdentityId = identityId,
        TokenString = token.Token,
        Issued = token.Issued,
        Expires = token.Expires,
        RefreshToken = token.RefreshToken,
        Scopes = token.Scopes.MakeString(" ")
    };
}

