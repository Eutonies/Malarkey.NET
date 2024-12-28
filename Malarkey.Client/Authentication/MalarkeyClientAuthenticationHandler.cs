using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
internal class MalarkeyClientAuthenticationHandler : AuthenticationHandler<MalarkeyClientAuthenticationSchemeOptions>
{

    public MalarkeyClientAuthenticationHandler(IOptionsMonitor<MalarkeyClientAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    private static readonly Regex _idTokenIndexRegex = new Regex($"{MalarkeyConstants.Authentication.IdentityCookieBaseName}\\.([0-9]+)");

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var endpoint = Context.GetEndpoint();
        if (endpoint == null)
            return AuthenticateResult.Fail("No endpoint info found");
        var authAttribute = endpoint.Metadata
            .OfType<MalarkeyAuthenticationAttribute>()
            .FirstOrDefault();
        if (authAttribute == null)
            return AuthenticateResult.NoResult();
        var profileCookie = Request.Cookies
            .Where(_ => _.Key == MalarkeyConstants.Authentication.ProfileCookieName)
            .Select(_ => _.Value.ToString())
            .FirstOrDefault();
        if (profileCookie == null)
            return AuthenticateResult.Fail("No profile cookie found");
        var profileToken = MalarkeyToken.ParseProfileToken(profileCookie);
        if (profileToken == null)
            return AuthenticateResult.Fail("Profile cookie could not be parsed as profile token");
        if (profileToken.ValidUntil < DateTime.Now)
            return AuthenticateResult.Fail("Profile cookie has expired");
        var profileTokenVerifies = await VerifySignature(profileToken);
        if(!profileTokenVerifies)
            return AuthenticateResult.Fail("Profile cookie signature does not verify");
        var identityCookies = Request.Cookies
            .Select(cook => (Matcher: _idTokenIndexRegex.Match(cook.Key), Value: cook.Value.ToString()))
            .Where(_ => _.Matcher.Success && _.Matcher.Groups.Count > 1)
            .Select(_ => (Index: int.Parse(_.Matcher.Groups[1].Value), _.Value))
            .ToList();
        var identityTokens = identityCookies
            .Select(cook => (cook.Index, Token: MalarkeyToken.ParseIdentityToken(cook.Value)))
            .Where(_ => _.Token != null && _.Token.ValidUntil > DateTime.Now)
            .OrderBy(_ => _.Index)
            .Select(_ => _.Token!)
            .ToList();
        identityTokens = (await FilterVerifiableTokens(identityTokens)).ToList();
        var identities = identityTokens
            .Select(_ => _.Identity)
            .ToList();
        if (authAttribute.IdentityProvider != null)
        {
            if(!identities.Any(_ => _.IdentityProvider == authAttribute.IdentityProvider))
               return AuthenticateResult.Fail($"No valid identity token found for ID Provider: {authAttribute.IdentityProvider}");
            if(authAttribute.Scopes != null)
            {
                var requestedScopes = authAttribute.Scopes
                    .Split(" ")
                    .Select(s => s.Trim().ToLower())
                    .ToHashSet();
                if(requestedScopes.Any())
                {
                    var relIdent = identities.First(_ => _.IdentityProvider == authAttribute.IdentityProvider);
                    var idpToken = relIdent.IdentityProviderTokenToUse;
                    if(idpToken == null)
                        return AuthenticateResult.Fail($"No valid access token found for ID Provider: {authAttribute.IdentityProvider}");
                    if(idpToken.Expires < DateTime.Now)
                    {
                        var refreshed = await RefreshToken(idpToken);
                        if(refreshed == null)
                            return AuthenticateResult.Fail($"Twas not possible to refresh IDP access token for provider: {authAttribute.IdentityProvider}");
                        relIdent = relIdent.WithToken(refreshed);
                        identities = identities
                            .Select(_ => _.IdentityId == relIdent.IdentityId ? relIdent : _)
                            .ToList();
                    }
                    var tokenScopes = idpToken.Scopes
                        .Select(_ => _.ToLower().Trim())
                        .ToHashSet();
                    var missingScopes = requestedScopes
                        .Except(tokenScopes)
                        .ToList();
                    if (missingScopes.Any())
                    {
                        return AuthenticateResult.Fail($"IDP access token for provider: {authAttribute.IdentityProvider} did not contain scopes: {missingScopes.Order().MakeString(" ")}");
                    }
                }

            }

        }



        var principal = profileToken.Profile.ToClaimsPrincipal(identities);
        var ticket = new AuthenticationTicket(principal, MalarkeyConstants.MalarkeyAuthenticationScheme);
        return AuthenticateResult.Success(ticket);
    }


    private async Task<IReadOnlyCollection<MalarkeyIdentityToken>> FilterVerifiableTokens(IEnumerable<MalarkeyIdentityToken> tokens)
    {
        var tasks = tokens
            .Select(_ => (Token: _, ResultTask: VerifySignature(_)))
            .ToList();
        await Task.WhenAll(tasks.Select(_ => _.ResultTask));
        var returnee = tasks
            .Where(_ => _.ResultTask.Result)
            .Select(_ => _.Token)
            .ToList();
        return returnee;
    }

    private Task<bool> VerifySignature(MalarkeyToken token)
    {
        throw new NotImplementedException();

    }


    private Task<IdentityProviderToken?> RefreshToken(IdentityProviderToken token)
    {
        throw new NotImplementedException();
    }


}
