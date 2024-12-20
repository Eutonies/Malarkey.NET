using Malarkey.Application.Security;
using Malarkey.Domain.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
internal class MalarkeyAuthenticationSessionHandler : IMalarkeyAuthenticationSessionHandler
{

    public Task<MalarkeyAuthenticationSession> InitSession(HttpContext context)
    {
        throw new NotImplementedException();
    }

    public Task<MalarkeyAuthenticationSession?> SessionForState(string state)
    {
        throw new NotImplementedException();
    }
}
