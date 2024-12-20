using Malarkey.Application.Security;
using Malarkey.Domain.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationSessionRepository : IMalarkeyAuthenticationSessionHandler
{
    public Task<MalarkeyAuthenticationSession> InitSession(MalarkeyOAuthIdentityProvider idProvider, string? forwarder)
    {
        throw new NotImplementedException();
    }

    public Task<MalarkeyAuthenticationSession?> SessionForState(string state)
    {
        throw new NotImplementedException();
    }
}
