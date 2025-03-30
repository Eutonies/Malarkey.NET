using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public interface IMalarkeyTokenCache
{
    Task<MalarkeyIdentityToken> SaveToken(MalarkeyIdentityToken token);
    Task<MalarkeyProfileToken> SaveToken(MalarkeyProfileToken token);

}
