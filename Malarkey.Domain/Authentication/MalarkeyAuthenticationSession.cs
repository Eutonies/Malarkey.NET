using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Authentication;
public record MalarkeyAuthenticationSession(
    long SessionId,
    string State,
    MalarkeyOAuthIdentityProvider IdProvider,
    string? Nonce,
    string? Forwarder,
    string CodeChallenge,
    string CodeVerifier,
    DateTime InitTime,
    DateTime? AuthenticatedTime,
    Guid? ProfileTokenId,
    Guid? IdentityTokenId,
    string Audience
    );
