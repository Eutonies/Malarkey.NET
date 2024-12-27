using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security.Persistence;
public interface IMalarkeyTokenRepository
{
    Task<MalarkeyIdentityToken> SaveToken(MalarkeyIdentityToken token);
    Task<MalarkeyProfileToken> SaveToken(MalarkeyProfileToken token);

}
