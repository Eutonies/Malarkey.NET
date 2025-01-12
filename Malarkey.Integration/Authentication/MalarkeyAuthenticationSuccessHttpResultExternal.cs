using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public record MalarkeyAuthenticationSuccessHttpResultExternal(
    string RedirectUrl,
    string ProfileToken,
    string IdentityToken,
    string? IdentityProviderAccessToken,
    string? ForwarderState,
    ILogger Logger
    ) : IResult
{
    private string? _forwardLocation;
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 200;
        var body = BodyString(RedirectUrl, ProfileToken, IdentityToken, IdentityProviderAccessToken, ForwarderState);
        await httpContext.Response.WriteAsJsonAsync(body);
    }


    private static string BodyString(string redirectUrl, string profileToken, string identityToken, string? idpAccessToken, string? forwarderState) =>
        $@"
            <html>
                <body onload='document.forms[""form""].submit()'>
                    <form name='form' action='{redirectUrl}' method='post'>
                        <input type='hidden' name='{MalarkeyConstants.AuthenticationSuccessParameters.ProfileTokenName}' value='{profileToken}'/>
                        <input type='hidden' name='{MalarkeyConstants.AuthenticationSuccessParameters.IdentityTokenName}' value='{identityToken}'/>
                        {(
                            idpAccessToken == null ? 
                                "" : 
                                ($"<input type='hidden' name='{MalarkeyConstants.AuthenticationSuccessParameters.IdentityProviderAccessTokenName}' value='{idpAccessToken}'/>")
            
                        )}
                        {(
                            forwarderState == null ?
                                "" :
                                ($"<input type='hidden' name='{MalarkeyConstants.AuthenticationSuccessParameters.ForwarderStateName}' value='{forwarderState}'/>")

                        )}

                    </form>
                </body>

            </html>


";

}
