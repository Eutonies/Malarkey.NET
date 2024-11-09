using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Common;
internal static class ApiExtensions
{

    public static WebApplication Map(this WebApplication app, ApiEndpointGroup apiGroup)
    {
        var group = app.MapGroup(apiGroup.Name);
        group.WithTags(apiGroup.Name)
             .WithOpenApi();
        if (apiGroup.AuthorizationPolicy != null)
            group.RequireAuthorization(apiGroup.AuthorizationPolicy);
        foreach (var endpointDef in apiGroup.Endpoints)
        {
            var conventionBuilder = endpointDef.Method switch
            {
                ApiHttpMethod.Get => group.MapGet(endpointDef.Pattern, endpointDef.Delegate),
                ApiHttpMethod.Post => group.MapPost(endpointDef.Pattern, endpointDef.Delegate),
                ApiHttpMethod.Put => group.MapPut(endpointDef.Pattern, endpointDef.Delegate),
                ApiHttpMethod.Delete => group.MapDelete(endpointDef.Pattern, endpointDef.Delegate),
                _ => group.MapPatch(endpointDef.Pattern, endpointDef.Delegate)
            };
            conventionBuilder.WithOpenApi();
            conventionBuilder.WithDescription(endpointDef.Description);
            if (endpointDef.AuthorizationPolicy != null)
                conventionBuilder.RequireAuthorization(endpointDef.AuthorizationPolicy);
        }
        return app; 
    }

}
