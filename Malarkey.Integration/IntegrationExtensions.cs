using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration;

public static class IntegrationExtensions
{
    public static bool IsAuthenticatedMicrosoftIdentity(this ClaimsIdentity ident) =>
        (ident.AuthenticationType == IntegrationConstants.MalarkeyIdProviders.Microsoft ||
        ident.Claims.Any(_ => _.Issuer.Contains("login.microsoftonline.com")))
        && ident.IsAuthenticated;

    public static bool IsAuthenticatedFacebookIdentity(this ClaimsIdentity ident) =>
        ident.AuthenticationType == IntegrationConstants.MalarkeyIdProviders.Facebook && ident.IsAuthenticated;

    public static bool IsAuthenticatedGoogleIdentity(this ClaimsIdentity ident) =>
        ident.AuthenticationType == IntegrationConstants.MalarkeyIdProviders.Google && ident.IsAuthenticated;

    public static bool IsAuthenticatedMicrosoftUser(this ClaimsPrincipal usr) =>
        usr.Identities.Any(_ => _.IsAuthenticatedMicrosoftIdentity());

    public static bool IsAuthenticatedFacebookUser(this ClaimsPrincipal usr) =>
        usr.Identities.Any(_ => _.IsAuthenticatedFacebookIdentity());
    public static bool IsAuthenticatedGoogleUser(this ClaimsPrincipal usr) =>
        usr.Identities.Any(_ => _.IsAuthenticatedGoogleIdentity());

}
