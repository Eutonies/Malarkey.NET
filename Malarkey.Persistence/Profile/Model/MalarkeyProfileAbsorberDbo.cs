using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class MalarkeyProfileAbsorberDbo
{
    public Guid ProfileId { get; set; }
    public Guid? Absorber { get; set; }
}
