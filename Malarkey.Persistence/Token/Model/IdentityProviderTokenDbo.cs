using Malarkey.Abstractions.Token;
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

    public IdentityProviderToken ToDomain() => new IdentityProviderToken(
        Token: TokenString,
        Issued: Issued,
        Expires: Expires,
        RefreshToken: RefreshToken
        );

}
