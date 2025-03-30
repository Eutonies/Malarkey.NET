using Malarkey.API.Common;
using Malarkey.API.Profile;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.SwaggerUI;
using Malarkey.Abstractions;
using Microsoft.Extensions.Logging;
using Malarkey.Abstractions.Util;
using Microsoft.Extensions.Options;

namespace Malarkey.API;
public static class DependencyInjectionApi
{

    public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(MalarkeyController).Assembly);
        return builder;
    }

    public static WebApplication UseApi(this WebApplication app, string? hostingBasePath)
    {
        var logger = app.Services.GetRequiredService<ILogger<MalarkeyController>>();
        app.UseSwagger(opts =>
        {
            opts.RouteTemplate = $"/{MalarkeyConstants.API.ApiPath}/" + "{documentName}/openapi.{extension:regex(^(json|ya?ml)$)}";
            logger.LogInformation($"Using swagger route template: {opts.RouteTemplate}");
        });
        app.UseSwaggerUI(opts =>
        {
            if(string.IsNullOrWhiteSpace(hostingBasePath))
            {
                opts.SwaggerEndpoint($"/{MalarkeyConstants.API.ApiPath}/v1/openapi.json", "v1");
            }
            else
            {
                opts.SwaggerEndpoint($"./{MalarkeyConstants.API.ApiPath}/v1/openapi.json", "v1");
                opts.RoutePrefix = hostingBasePath;
            }
        });
        app.MapControllers()
            .DisableAntiforgery();
        return app;
    }

}
