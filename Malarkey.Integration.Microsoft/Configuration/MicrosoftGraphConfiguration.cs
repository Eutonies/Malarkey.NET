using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Microsoft.Configuration;

public class MicrosoftGraphConfiguration
{
    public string BaseUrl { get; set; }
    public bool RequestAppToken { get; set; }
    public string[] Scopes { get; set; }

}
