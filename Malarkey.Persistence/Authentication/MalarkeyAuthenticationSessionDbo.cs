using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationSessionDbo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SessionId { get; set; }
    public string State { get; set; }
    public string IdProvider { get; set; }
    public string? Nonce { get; set; }
    public string? Forwarder { get; set; }
    public string CodeVerifier { get; set; }
    public string CodeChallenge { get; set; }
    public DateTime InitTime { get; set; }
    public DateTime? AuthenticatedTime { get; set; }
    public string? ProfileTokenId { get; set; }
    public string? IdentityTokenId { get; set; }



}
