using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class HttpExtensions
{

    public static MalarkeyAuthenticationRequestParameters ToAuthenticationParameters(this HttpRequest req)
    {
        var idpMap = MalarkeyIdentityProviders.AllProviders
            .ToDictionarySafe(_ => _.ToString().ToLower());
        var idp = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.IdProviderName)
            .Select(_ => _.Value.ToString().ToLower())
            .Select(_ => new {Value = idpMap.TryGetValue(_, out var idprov) ? (MalarkeyIdentityProvider?) idprov : null })
            .FirstOrDefault()?.Value;
        var forwarder = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        var scopes = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ScopesName)
            .Select(_ => _.Value.ToString().Split(" "))
            .FirstOrDefault();
        var forwarderState = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderStateName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        var existingProfileId = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.ExistingProfileIdName)
            .Select(_ => _.Value.ToString())
            .Select(_ => Guid.TryParse(_, out var profId) ? (Guid?)profId : null)
            .FirstOrDefault();
        var alwaysChallenge = req.Query
            .Where(_ => _.Key == MalarkeyConstants.AuthenticationRequestQueryParameters.AlwaysChallengeName)
            .Select(_ => _.Value.ToString().ToLower().Contains("true"))
            .Select(_ => new { Value = _ })
            .FirstOrDefault()?.Value;
        var returnee = new MalarkeyAuthenticationRequestParameters(
            Provider: idp,
            Scopes: scopes,
            Forwarder: forwarder,
            ForwarderState: forwarderState,
            ExistingProfileId: existingProfileId,
            AlwaysChallenge: alwaysChallenge
            );
        return returnee;
    }

}
