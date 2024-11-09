using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Common;
public enum ApiHttpMethod
{
    Get = 1,
    Put = 2,
    Post = 3,
    Patch = 4,
    Delete = 5,
    Unknown = 100

}

public static class ApiHttpMethodExtensions
{
    public static HttpMethod Map(this ApiHttpMethod method) => method switch
    {
        ApiHttpMethod.Get => HttpMethod.Get,
        ApiHttpMethod.Put => HttpMethod.Put,
        ApiHttpMethod.Post => HttpMethod.Post,
        ApiHttpMethod.Patch => HttpMethod.Patch,
        ApiHttpMethod.Delete => HttpMethod.Delete,
        _ => HttpMethod.Get
    };

    private static readonly Dictionary<HttpMethod, ApiHttpMethod> MethodMap = new List<(HttpMethod, ApiHttpMethod)>
    {
        (HttpMethod.Get, ApiHttpMethod.Get),
        (HttpMethod.Put, ApiHttpMethod.Put),
        (HttpMethod.Post, ApiHttpMethod.Post),
        (HttpMethod.Patch, ApiHttpMethod.Patch),
        (HttpMethod.Delete, ApiHttpMethod.Delete)
    }.ToDictionary(_ => _.Item1, _ => _.Item2);

    public static ApiHttpMethod Map(this HttpMethod method) => MethodMap.GetValueOrDefault(method, ApiHttpMethod.Unknown);

    public static ApiHttpMethod ParseAsApiHttpMethod(this string methodName) => Enum.TryParse(methodName, true, out ApiHttpMethod result) ? result : ApiHttpMethod.Unknown;


}