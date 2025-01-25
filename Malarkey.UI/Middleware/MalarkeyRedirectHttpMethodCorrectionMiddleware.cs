using Malarkey.UI.Pages.Profile;
using Microsoft.AspNetCore.Http.Extensions;

namespace Malarkey.UI.Middleware;

public class MalarkeyRedirectHttpMethodCorrectionMiddleware
{
    private readonly RequestDelegate _next;

    public MalarkeyRedirectHttpMethodCorrectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.HasValue && context.Request.Path.Value.ToLower().Contains(ProfileIdentityConnectionSucceededPage.SucceededPagePath.ToLower()))
        {
            context.Request.Method = "GET";
        }
        await _next(context);
    }

}
