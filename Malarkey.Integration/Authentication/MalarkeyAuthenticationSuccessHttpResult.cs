using Malarkey.Abstractions;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
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
    string? IdentityProviderAccessToken
    ) : IResult
{
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 302;
        httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.ProfileTokenName, ProfileToken);
        httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityTokenName, IdentityToken);
        if(IdentityProviderAccessToken != null)
        {
            httpContext.Response.Headers.Append(MalarkeyConstants.AuthenticationSuccessQueryParameters.IdentityProviderAccessTokenName, IdentityProviderAccessToken);
        }
        httpContext.Response.Headers.Location = BuildRedirectString();
        return Task.CompletedTask;
    }

    private string BuildRedirectString()
    {
        var returnee = new StringBuilder();
        returnee.Append(RedirectUrl);
        returnee.Append("?profileToken=" + ProfileToken.UrlEncoded());
        returnee.Append("&identityToken=" + IdentityToken.UrlEncoded());
        if (IdentityProviderAccessToken != null)
        {
            returnee.Append("&idpToken=" + IdentityProviderAccessToken.UrlEncoded());
        }
        return returnee.ToString();
    }

}
