using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
internal static class MalarkeySecurityConstants
{
    internal const string TokenIssuer = "eutonies.com/malarkey";
    internal const string TokenAlgorithm = "RS256";
    internal const string TokenType = "JWT";
    internal static readonly TimeSpan TokenLifeTime = TimeSpan.FromHours(1);


}
