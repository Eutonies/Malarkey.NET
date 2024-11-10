using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security.Persistence;
internal interface IMalarkeyTokenRepository
{
    Task<MalarkeyToken> CreateToken();


}
