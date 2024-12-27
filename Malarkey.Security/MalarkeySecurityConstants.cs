using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
internal static class MalarkeySecurityConstants
{
    internal static readonly TimeSpan TokenLifeTime = TimeSpan.FromHours(1);
    internal static DateTime Now => DateTime.Now;

}
