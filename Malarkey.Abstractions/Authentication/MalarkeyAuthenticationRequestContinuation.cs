using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationRequestContinuation(
    string Path,
    IReadOnlyCollection<(string Name, string Value)> QueryParameters,
    string? Body
    )
{
    public static MalarkeyAuthenticationRequestContinuation From(HttpRequest request)
    {
        var queryParams = request.Query
               .Where(_ => !string.IsNullOrWhiteSpace(_.Value.ToString()))
               .Select(_ => (Name: _.Key, Value: _.Value.ToString()))
               .ToList();
        var paramsMap = queryParams
            .ToDictionarySafe(_ => _.Name, _ => _.Value);

        var returnee = new MalarkeyAuthenticationRequestContinuation(
            Path: paramsMap.TryGetValue(MalarkeyConstants.AuthenticationRequestQueryParameters.ForwarderName, out var forwarder) ? forwarder : request.Path.Value!,
            QueryParameters: queryParams,
            Body: null
        );
        return returnee;
    } 



}
