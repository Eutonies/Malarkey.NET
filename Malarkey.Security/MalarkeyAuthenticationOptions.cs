using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
public class MalarkeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public MalarkeyAuthenticationOptions()
    {
        ClaimsIssuer = MalarkeySecurityConstants.TokenIssuer;
    }


}
