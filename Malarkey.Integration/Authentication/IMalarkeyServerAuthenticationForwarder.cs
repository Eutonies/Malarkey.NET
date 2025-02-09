using Malarkey.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;

public interface IMalarkeyServerAuthenticationForwarder
{
    Task Forward(MalarkeyAuthenticationSession session, Guid profileId, HttpContext context);

    string StateVerifierFor(string state);
    string StateFor(string verifier);

}
