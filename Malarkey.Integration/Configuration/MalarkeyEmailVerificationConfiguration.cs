using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Configuration;
public class MalarkeyEmailVerificationConfiguration
{
    public string ApiBaseAddress { get; set; }

    public string ApiClientId { get; set; }

    public string ApiToken { get; set; }

    public string Sender { get; set; }

    public string VerifyEmailUrl { get; set; }


}
