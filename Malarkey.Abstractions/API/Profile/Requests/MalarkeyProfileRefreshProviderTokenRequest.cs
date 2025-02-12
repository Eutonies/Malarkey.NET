using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile.Requests;
public record MalarkeyProfileRefreshProviderTokenRequest(
    MalarkeyIdentityProviderDto IdentityProvider,
    string AccessToken,
    string ClientCertificate
    );
