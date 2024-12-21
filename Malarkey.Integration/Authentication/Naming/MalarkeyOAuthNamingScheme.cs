using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.Naming;
public class MalarkeyOAuthNamingScheme
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string ResponseType { get; set; }
    public string RedirectUri { get; set; }
    public string ResponseMode { get; set; }
    public string Scope { get; set; }
    public string State { get; set; }
    public string Nonce { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeChallengeMethod { get; set; }

    public string RedemptionGrantType { get; set; }
    public string RedemptionCode { get; set; }
    public string RedemptionCodeVerifier { get; set; }

    protected string OrDefault(string? configName, string defaultName) => configName ?? defaultName;

}
