using Malarkey.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;

namespace Malarkey.Integration.Authentication;
public class MalarkeyAuthenticationEvents
{
    /// <summary>
    /// Invoked when the client needs to be redirected to the sign in url.
    /// </summary>
    public Func<RedirectContext<MalarkeyServerAuthenticationHandlerOptions>, Task> OnRedirectToChallenge { get; set; } = context =>
    {
        context.Response.StatusCode = 302
;       context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    /// <summary>
    /// Invoked when the client needs to be redirected to the access denied url.
    /// </summary>
    public Func<RedirectContext<MalarkeyServerAuthenticationHandlerOptions>, Task> OnRedirectToLogin { get; set; } = context =>
    {
        context.Response.StatusCode = 302;
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };


    /// <summary>
    /// Invoked when the client needs to be redirected to the access denied url.
    /// </summary>
    public Func<HttpContext, string, Task> OnFailure { get; set; } = async (context, error) =>
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync(error);
    };


    public Func<RedirectContext<MalarkeyServerAuthenticationHandlerOptions>, Task> OnRedirectUponCompletion { get; set; } = context =>
    {
        context.Response.StatusCode = 302; 
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };





}
