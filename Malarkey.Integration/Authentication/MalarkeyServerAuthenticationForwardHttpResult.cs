using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Common;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Malarkey.Integration.Authentication;
internal class MalarkeyServerAuthenticationForwardHttpResult : MalarkeyHttpForwardPostResult
{

    private readonly IReadOnlyCollection<(string Name, string Value)> _postValues;
    protected override IReadOnlyCollection<(string Name, string Value)> PostValues => _postValues;

    public MalarkeyServerAuthenticationForwardHttpResult(
        MalarkeyAuthenticationSession session,
        string profileToken,
        IReadOnlyCollection<string> identityTokens,
        ILogger logger
        ) : base(session.SendTo, [])
    {
        var _returnState = session.State;
        if (session.EncryptState)
        {
            logger.LogInformation($"Will encrypt state using audience certificate: {session.Audience}");
            var receiverCertBytes = Convert.FromBase64String(session.Audience);
            logger.LogInformation($"  That's {receiverCertBytes.Length} bytes worth of certificate");
            var recieverCertificate = X509CertificateLoader.LoadCertificate(receiverCertBytes);
            logger.LogInformation($"  Loaded certificate with friendly name: {recieverCertificate.FriendlyName}");
            var encryptedStateBytes = recieverCertificate.PublicKey.GetRSAPublicKey()!
                .Encrypt(
                  data: UTF8Encoding.UTF8.GetBytes(session.State), 
                  padding: MalarkeyConstants.RSAPadding
            );
            var encryptedState = Convert.ToBase64String(encryptedStateBytes);
            _returnState = encryptedState;
            logger.LogInformation($"  Return state will be: {_returnState}");
        }
        _postValues = new List<(string Name, string Value)>
        {
            (MalarkeyConstants.AuthenticationSuccessParameters.StateName, _returnState),
            (MalarkeyConstants.AuthenticationSuccessParameters.ProfileTokenName, profileToken)
        }.Concat(
            identityTokens
              .Select((tok, indx) => (MalarkeyConstants.AuthenticationSuccessParameters.IdentityTokenName(indx), tok))
         ).ToList();

    }
}
