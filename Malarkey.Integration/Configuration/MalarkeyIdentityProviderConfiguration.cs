using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyIdentityProviderConfiguration
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string? ResponseType { get; set; }
    public string? CodeChallengeMethod { get; set; }

}
