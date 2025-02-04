using Malarkey.Abstractions;
using Malarkey.Abstractions.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Configuration;
public class MalarkeyClientConfiguration
{
    public const string ConfigurationElementName = "MalarkeyClient";

    public string MalarkeyServerBaseAddress { get; set; }
    public string ClientCertificateFile { get; set; }
    public string ClientCertificatePassword { get; set; }

    public string? ClientAuthenticatedPath { get; set; }
    public string ClientAuthenticatedPathToUse => ClientAuthenticatedPath ?? MalarkeyConstants.Client.Paths.DefaultAuthenticationForwarderPath;
    public string ClientServerBasePath { get; set; }
    public string FullClientServerUrl => $"{ClientServerBasePath}{ClientAuthenticatedPathToUse}";

    public string LogLevel { get; set; } = "Debug";

    public LogLevel LogLevelToUse => Enum.TryParse<LogLevel>(LogLevel, out var lev) ? lev : Microsoft.Extensions.Logging.LogLevel.Debug;

    private X509Certificate2? _clientCertificate;
    public X509Certificate2 ClientCertificate => _clientCertificate ??= X509CertificateLoader.LoadPkcs12FromFile(ClientCertificateFile, password: ClientCertificatePassword);


    private string? _clientCertificateString;
    public string ClientCertificateString => _clientCertificateString ??= ClientCertificate.ExportCertificatePem();



}
