using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandlerOptions : AuthenticationSchemeOptions
{
    public bool EnableMicrosoft { get; set; }
    public bool EnableGoogle { get; set; }
    public bool EnableFacebook { get; set; }
    public bool EnableSpotify { get; set; }


}
