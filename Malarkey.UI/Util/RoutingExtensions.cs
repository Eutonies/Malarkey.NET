using Malarkey.UI.Pages.Authenticate;

namespace Malarkey.UI.Util;

public static class RoutingExtensions
{

    public static string? RouteOf(this Type typ)
    {
        var attrs = typ.CustomAttributes;
        var routed = attrs
            .Where(_ => _.AttributeType == typeof(Microsoft.AspNetCore.Components.RouteAttribute))
            .FirstOrDefault();
        if (routed == null)
            return null;
        var route = routed.ConstructorArguments
            .Select(_ => _.Value?.ToString())
            .FirstOrDefault();
        return route;
    }

}
