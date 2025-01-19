using Malarkey.Abstractions.Profile;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationRequestParameters(
    MalarkeyIdentityProvider? Provider = null,
    string[]? Scopes = null,
    string? Forwarder = null,
    string? ForwarderState = null,
    Guid? ExistingProfileId = null,
    bool? AlwaysChallenge = null
    )
{

}
