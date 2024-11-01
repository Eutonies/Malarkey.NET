using Malarkey.Integration.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Facebook.Configuration;
public class FacebookIntegrationIdentityConfiguration : IdentityProviderConfiguration
{
    public string AppId { get; set; }

}
