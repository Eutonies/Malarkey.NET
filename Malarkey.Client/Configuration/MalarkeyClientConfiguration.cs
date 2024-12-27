using Malarkey.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Configuration;
public class MalarkeyClientConfiguration
{
    public string MalarkeyServerBaseAddress { get; set; }
    public string MalarkeyClientPublicKeyFile { get; set; }

    private string? _clientPublicKey;
    public string MalarkeyClientPublicKey { get => _clientPublicKey ??= File.ReadAllText(MalarkeyClientPublicKeyFile).CleanCertificate(); }


}
