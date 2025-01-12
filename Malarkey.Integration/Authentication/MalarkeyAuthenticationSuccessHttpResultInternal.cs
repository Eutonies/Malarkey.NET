using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public record MalarkeyAuthenticationSuccessHttpResultInternal(
    MalarkeyAuthenticationRequestContinuation Continuation,
    ILogger Logger
    ) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await Task.CompletedTask;
        httpContext.Response.StatusCode = 302;
        var url = new StringBuilder($"{Continuation.Path}");
        if (Continuation.QueryParameters.Any())
            url.Append("?" + (Continuation.QueryParameters
                .Select(par => $"{par.Name}={par.Value.UrlEncoded()}")
                .MakeString("&")));
        httpContext.Response.Redirect(url.ToString());
    }




}
