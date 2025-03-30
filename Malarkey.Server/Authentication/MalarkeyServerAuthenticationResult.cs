using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Server.Authentication;
public abstract record MalarkeyServerAuthenticationResult {}

public sealed record MalarkeyServerAuthenticationSuccessResult(
    MalarkeyProfile Profile,
    MalarkeyProfileIdentity AuthenticatedIdentity,
    MalarkeyProfileToken ProfileToken,
    MalarkeyIdentityToken IdentityToken
    ) : MalarkeyServerAuthenticationResult;


public sealed record MalarkeyServerAuthenticationFailureResult(
    string ErrorMessage
    ) : MalarkeyServerAuthenticationResult;

