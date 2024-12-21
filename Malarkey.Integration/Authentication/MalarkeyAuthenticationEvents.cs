using Malarkey.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public class MalarkeyAuthenticationEvents
{
    /// <summary>
    /// Invoked when the client needs to be redirected to the sign in url.
    /// </summary>
    public Func<RedirectContext<MalarkeyServerAuthenticationHandlerOptions>, Task> OnRedirectToLogin { get; set; } = context =>
    {
        context.Response.StatusCode = 302
;       context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };


    /// <summary>
    /// Invoked when the client needs to be redirected to the access denied url.
    /// </summary>
    public Func<RedirectContext<MalarkeyServerAuthenticationHandlerOptions>, Task> OnRedirectToAccessDenied { get; set; } = context =>
    {
        context.Response.StatusCode = 403;
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };




}
