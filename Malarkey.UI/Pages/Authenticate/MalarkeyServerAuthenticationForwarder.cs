using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Integration.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Malarkey.UI.Pages.Authenticate;

public class MalarkeyServerAuthenticationForwarder : IMalarkeyServerAuthenticationForwarder
{

    private readonly ILogger<MalarkeyServerAuthenticationForwarder> _logger;

    public MalarkeyServerAuthenticationForwarder(ILogger<MalarkeyServerAuthenticationForwarder> logger)
    {
        _logger = logger;
    }

    private readonly Aes _stateVerifier = Aes.Create();

    public async Task Forward(MalarkeyAuthenticationSession session, Guid profileId, HttpContext context)
    {
        await Task.CompletedTask;
        _logger.LogInformation($"Will generate state verifier for state: {session.State}");
        var verifier = StateVerifierFor(session.State);
        _logger.LogInformation($"  Generated verifier: {verifier}");
        var targetUrl = ForwarderPage.UrlFor(session.State, verifier, profileId);
        _logger.LogInformation($"  Re-directing to forward URL: {targetUrl}");
        // Status code 303: redirect to Headers.Location with Http Method GET
        context.Response.StatusCode = 303;
        context.Response.Headers.Location = targetUrl;
    }

    public string StateFor(string verifier)
    {
        var bytes = Convert.FromBase64String(verifier);
        using(var inputStream = new MemoryStream(bytes))
        {
            using (var cryptStream = new CryptoStream(
                stream: inputStream,
                _stateVerifier.CreateDecryptor(),
                mode: CryptoStreamMode.Read
                ))
            {
                using var reader = new StreamReader(cryptStream);
                var readString = reader.ReadToEnd();
                return readString;
            }
        }
    }

    public string StateVerifierFor(string state)
    {
        using (var outputStream = new MemoryStream())
        {
            using (var cryptStream = new CryptoStream(
                stream: outputStream,
                transform: _stateVerifier.CreateEncryptor(),
                mode: CryptoStreamMode.Write))
            {
                using(var writer = new StreamWriter(cryptStream))
                {
                    writer.Write(state);
                }
            }
            var encrypted = outputStream.ToArray();
            var returnee = Convert.ToBase64String(encrypted);
            return returnee;
        }


    }
}
