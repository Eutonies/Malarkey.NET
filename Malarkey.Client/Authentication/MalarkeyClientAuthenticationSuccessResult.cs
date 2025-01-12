using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
public record MalarkeyClientAuthenticationSuccessResult(
    MalarkeyAuthenticationRequestContinuation Continuation
    ) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 302;
        httpContext.Response.Headers.Location = BuildRedirectString();
        if (Continuation.Body != null)
            httpContext.Response.WriteAsync(Continuation.Body);
        return Task.CompletedTask;
    }

    private string BuildRedirectString()
    {
        var returnee = new StringBuilder();
        returnee.Append(Continuation.Path);
        if(Continuation.QueryParameters.Any())
        {
            var first = Continuation.QueryParameters.First();
            returnee.Append($"?{first.Name}={first.Value.UrlEncoded()}");
            foreach (var nxt in Continuation.QueryParameters.Skip(1))
                returnee.Append($"&{nxt.Name}={nxt.Value.UrlEncoded()}");
        }
        return returnee.ToString();
    }

}
