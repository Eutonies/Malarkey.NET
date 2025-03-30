using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
internal class MalarkeyClientAuthenticationAttributeMiddleware
{
    private readonly RequestDelegate _next;

    public MalarkeyClientAuthenticationAttributeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var authAttr = endpoint.Metadata
                .OfType<MalarkeyAuthenticationAttribute>()
                .FirstOrDefault();
            if (authAttr != null)
            {
                var updatedMetaData = new EndpointMetadataCollection(endpoint.Metadata.Append(
                    new AuthorizeAttribute
                    {
                        AuthenticationSchemes = MalarkeyConstants.MalarkeyAuthenticationScheme
                    }));
                var routePattern = RoutePatternFactory.Parse("/");
                var order = 1;
                if (endpoint is RouteEndpoint re)
                {
                    routePattern = re.RoutePattern;
                    order = re.Order;
                }
                endpoint = new RouteEndpoint(
                    requestDelegate: endpoint.RequestDelegate!,
                    routePattern: routePattern,
                    order: order,
                    metadata: updatedMetaData,
                    displayName: endpoint.DisplayName
                    );
                context.SetEndpoint(endpoint);
            }
        }


        await _next(context);
    }

}
