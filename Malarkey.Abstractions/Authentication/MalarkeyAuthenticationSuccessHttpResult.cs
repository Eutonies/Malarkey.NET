using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationSuccessHttpResult(
    MalarkeyAuthenticationSession Session,
    string ProfileToken,
    IReadOnlyCollection<string> IdentityTokens,
    ILogger Logger
    ) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await Task.CompletedTask;
        httpContext.Response.StatusCode = 302;
        httpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName, ProfileToken);
        var identList = IdentityTokens.ToList();
        for(var identIndx = 0; identIndx < identList.Count; identIndx++)
           httpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.IdentityCookieName(identIndx), identList[identIndx]);
        var url = new StringBuilder($"{Session.SendTo}");
        if(Session.IsInternal && Session.RequestParameters.Any())
        {
            url.Append("?" + Session.RequestParameters
                 .Select(_ => $"{_.Name}=${_.Value.UrlEncoded()}")
                 .MakeString("&")
            );
        }
        httpContext.Response.Redirect(url.ToString(), permanent: false, preserveMethod: false);
    }




}
