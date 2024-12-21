using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandlerOptions : AuthenticationSchemeOptions
{
    public string AccessDeniedUrl { get; set; }
    public string PublicKey { get; set; }

}
