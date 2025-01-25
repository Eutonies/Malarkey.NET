using Malarkey.UI.Pages.Profile;
using Microsoft.AspNetCore.Http.Extensions;

namespace Malarkey.UI.Middleware;

internal class MalarkeyRequestLoggingMiddleware 
{

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public MalarkeyRequestLoggingMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Log(logger => logger.LogInformation($"Received request: {context.Request.GetDisplayUrl()}"));
        try {
            await _next(context);
        }
        catch (Exception ex) 
        {
            Log(logger => logger.LogError(ex ,$"Error during handling of request: {context.Request.GetDisplayUrl()}"));
        }
        Log(
            logger => {
                logger.LogInformation($"Finished handling request: {context.Request.GetDisplayUrl()}");
                logger.LogInformation($"  Returning status code: {context.Response.StatusCode} of type: {context.Response.GetType()}");
        });

    }

    private void Log(Action<ILogger<MalarkeyRequestLoggingMiddleware>> logAction) 
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MalarkeyRequestLoggingMiddleware>>();
        logAction(logger);
    }




}