using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model;
internal class MalarkeyTokenDbo
{
    [Key]
    public Guid TokenId { get; set; }
    public string TokenType { get; set; }
    public Guid ProfileId { get; set; }
    public Guid? IdentityId { get; set; }
    public string IssuedTo { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime? RevokedAt { get; set; }

}
