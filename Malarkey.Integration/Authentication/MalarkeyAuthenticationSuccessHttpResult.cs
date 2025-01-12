using Malarkey.Abstractions;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public record MalarkeyAuthenticationSuccessHttpResult(
    string RedirectUrl,
    string ProfileToken,
    string IdentityToken,
    string? IdentityProviderAccessToken,
    string? ForwarderState,
    ILogger Logger
    ) : IResult
{
    private string? _forwardLocation;
    public string ForwardLocation => _forwardLocation ??= BuildRedirectString();
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 302;
        /*httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.ProfileTokenName, ProfileToken);
        httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityTokenName, IdentityToken);
        if(IdentityProviderAccessToken != null)
        {
            httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityProviderAccessTokenName, IdentityProviderAccessToken);
        }
        if (ForwarderState != null)
        {
            httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.ForwarderStateName, ForwarderState);
        }*/
        var newLocation = BuildRedirectString();
        Logger.LogInformation($"Authentication success redirect result redirecting to: {newLocation}");
        //httpContext.Response.Headers.Location = newLocation;
        httpContext.Response.Redirect(newLocation);
        return Task.CompletedTask;
    }

    private string BuildRedirectString()
    {
        var returnee = new StringBuilder();
        returnee.Append(RedirectUrl);
        //returnee.Append($"?{MalarkeyConstants.AuthenticationSuccessQueryParameters.ProfileTokenName}={ProfileToken.UrlEncoded()}");
        /*returnee.Append($"&{MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityTokenName}={IdentityToken.UrlEncoded()}");
        if (IdentityProviderAccessToken != null)
        {
            returnee.Append($"&{MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityProviderAccessTokenName}={IdentityProviderAccessToken.UrlEncoded()}");
        }
        if (ForwarderState != null)
            returnee.Append($"&{MalarkeyConstants.AuthenticationSuccessQueryParameters.ForwarderStateName}={ForwarderState.UrlEncoded()}");*/
        return returnee.ToString();
    }

}
