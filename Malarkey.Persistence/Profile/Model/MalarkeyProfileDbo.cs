using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class MalarkeyProfileDbo
{
    [Key]
    public Guid ProfileId { get; set; }
    public string ProfileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? AbsorbedBy { get; set; }

}
