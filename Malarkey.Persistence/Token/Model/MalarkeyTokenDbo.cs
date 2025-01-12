using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model;
internal class MalarkeyTokenDbo
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid TokenId { get; set; }
    public MalarkeyTokenTypeDbo TokenType { get; set; }
    public Guid ProfileId { get; set; }
    public Guid? IdentityId { get; set; }
    public string IssuedTo { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime? RevokedAt { get; set; }

    public MalarkeyIdentityToken ToIdentityToken(MalarkeyProfileIdentity ident) => new MalarkeyIdentityToken(
        TokenId: TokenId,
        IssuedTo: IssuedTo,
        IssuedAt: IssuedAt.ToUniversalTime(),
        ValidUntil: ValidUntil.ToUniversalTime(),
        Identity: ident
        );

    public MalarkeyProfileToken ToProfileToken(MalarkeyProfile prof) => new MalarkeyProfileToken(
        TokenId: TokenId,
        IssuedTo: IssuedTo,
        IssuedAt: IssuedAt.ToUniversalTime(),
        ValidUntil: ValidUntil.ToUniversalTime(),
        Profile: prof
        );
}

internal static class MalarkeyTokenDboExtensions
{
    public static MalarkeyTokenDbo ToDbo(this MalarkeyIdentityToken token) => new MalarkeyTokenDbo
    {
        IdentityId = token.Identity.IdentityId,
        ProfileId = token.Identity.ProfileId,
        IssuedTo = token.IssuedTo,
        IssuedAt = token.IssuedAt.ToLocalTime(),
        ValidUntil = token.ValidUntil.ToLocalTime(),
        TokenType = MalarkeyTokenTypeDbo.Identity
    };
    public static MalarkeyTokenDbo ToDbo(this MalarkeyProfileToken token) => new MalarkeyTokenDbo
    {
        ProfileId = token.Profile.ProfileId,
        IssuedTo = token.IssuedTo,
        IssuedAt = token.IssuedAt.ToLocalTime(),
        ValidUntil = token.ValidUntil.ToLocalTime(),
        TokenType = MalarkeyTokenTypeDbo.Profile
    };

}

