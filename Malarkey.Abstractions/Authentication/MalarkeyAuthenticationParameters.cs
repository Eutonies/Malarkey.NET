using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParDefs = Malarkey.Abstractions.MalarkeyConstants.AuthenticationRequestQueryParameters;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationParameters(
    bool IsInternal,
    string? RequestedSendTo,
    MalarkeyIdentityProvider? RequestedIdProvider,
    string? RequestState,
    string[]? RequestedScopes,
    Guid? ExistingProfileId,
    bool AlwaysChallenge
    )
{
    public string ProduceAuthenticationString(string? basePath = null) =>
        $"{basePath?.Pipe(bp => bp + "/") ?? ""}{MalarkeyConstants.Authentication.ServerAuthenticationPath}" + (QueryParameterValues switch
        {
            IReadOnlyCollection<(string Key, string Value)> coll when coll.Any() => "?" + (coll
               .Select(_ => $"{_.Key}={_.Value.UrlEncoded()}")
               .MakeString("&")),
            _ => ""
        });

    public IReadOnlyCollection<(string Key, string Value)> QueryParameterValues => new List<(string, string?)>
        {
            (ParDefs.SendToName, RequestedSendTo),
            (ParDefs.IdProviderName, RequestedIdProvider?.ToString()),
            (ParDefs.SendToStateName, RequestState),
            (ParDefs.ScopesName, RequestedScopes?.MakeString(" ")),
            (ParDefs.ExistingProfileIdName, ExistingProfileId?.ToString()),
            (ParDefs.AlwaysChallengeName, AlwaysChallenge ? true.ToString() : null)
        }
        .Where(_ => _.Item2 != null)
        .Select(_ => (_.Item1, _.Item2!))
        .ToList();


}
