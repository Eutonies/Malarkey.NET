using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
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

