using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.Naming;
internal static class MalarkeyGoogleOAuthNamingScheme
{
    private static MalarkeyOAuthNamingScheme? _instance;
    internal static MalarkeyOAuthNamingScheme Init(MalarkeyOAuthNamingSchemeConfiguration config) =>
        _instance ??= new MalarkeyOAuthNamingScheme
        {
            ClientId = "client_id",
            ClientSecret = "client_secret",
            ResponseType = "response_type",
            RedirectUri = "redirect_uri",
            ResponseMode = "response_mode",
            Scope = "scope",
            State = "state",
            Nonce = "nonce",
            CodeChallenge = "code_challenge",
            CodeChallengeMethod = "code_challenge_method",

            RedemptionGrantType = "grant_type",
            RedemptionCode = "code",
            RedemptionCodeVerifier = "code_verifier"
        };


}
