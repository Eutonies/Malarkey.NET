using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationIdpSession(
    long IdpSessionId,
    long SessionId,
    MalarkeyIdentityProvider IdProvider,
    string? Nonce,
    string CodeChallenge,
    string CodeVerifier,
    DateTime InitTime,
    DateTime? AuthenticatedTime,
    string[] Scopes
    );
