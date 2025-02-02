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
        var urlHolder = new StringBuilder($"{Session.SendTo}");
        if(Session.IsInternal && Session.RequestParameters.Any())
        {
            urlHolder.Append("?" + Session.RequestParameters
                 .Select(_ => $"{_.Name}=${_.Value.UrlEncoded()}")
                 .MakeString("&")
            );
        }
        var url = urlHolder.ToString();
        httpContext.Response.StatusCode = 302;
        DebugLog($"Malarkey Authentication Success Result - HTTP Status Code: {httpContext.Response.StatusCode}");
        DebugLog($"URL: {url}");
        DebugLog($"   Profile token: {MalarkeyConstants.Authentication.ProfileCookieName}={ProfileToken}");
        httpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName, ProfileToken);
        DebugLog($"   Identity tokens:");
        var identList = IdentityTokens.ToList();
        for(var identIndx = 0; identIndx < identList.Count; identIndx++) {
            var cookieName = MalarkeyConstants.Authentication.IdentityCookieName(identIndx);
            var idTok = identList[identIndx];
           httpContext.Response.Cookies.Append(cookieName, idTok);
           DebugLog($"    {cookieName}={idTok}");
        }
        httpContext.Response.Redirect(url, permanent: false, preserveMethod: false);
    }


    private void DebugLog(string str) => 
      Logger.LogInformation(str);




}
