using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class MalarkeyIdentityDbo
{
    [Key]
    public Guid IdentityId { get; set; }
    public Guid ProfileId { get; set; }
    public string Provider { get; set; }
    public string ProviderId { get; set; }
    public string IdentityName { get; set; }
    public string? PreferredName { get; set; }
    public string? MiddleNames { get; set; }
    public string? LastName { get; set; }

}
