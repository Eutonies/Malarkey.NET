using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.Naming;
internal static class MalarkeyGoogleOAuthNamingScheme
{
    private static MalarkeyOAuthNamingScheme? _instance;
    internal static MalarkeyOAuthNamingScheme Init(MalarkeyOAuthNamingSchemeConfiguration? config) =>
        _instance ??= new MalarkeyOAuthNamingScheme
        {
            ClientId = config?.ClientId ?? "client_id",
            ClientSecret = config?.ClientSecret ?? "client_secret",
            ResponseType = config?.ResponseType ?? "response_type",
            RedirectUri = config?.RedirectUri ?? "redirect_uri",
            ResponseMode = config?.ResponseMode ?? "response_mode",
            Scope = config?.Scope ?? "scope",
            State = config?.State ?? "state",
            Nonce = config?.Nonce ?? "nonce",
            CodeChallenge = config?.CodeChallenge ?? "code_challenge",
            CodeChallengeMethod = config?.CodeChallengeMethod ?? "code_challenge_method",

            RedemptionGrantType = config?.RedemptionGrantType ?? "grant_type",
            RedemptionCode = config?.RedemptionCode ?? "code",
            RedemptionCodeVerifier = config?.RedemptionCodeVerifier ?? "code_verifier"
        };


}
