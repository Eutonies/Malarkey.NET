using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public interface IMalarkeyClientTokenIssuer
{
    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile);
    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(MalarkeyProfileIdentity identity);

    public async Task<IReadOnlyCollection<(MalarkeyIdentityToken Token, string TokenString)>> IssueTokens(
        IEnumerable<MalarkeyProfileIdentity> tokens)
    {
        if (!tokens.Any())
            return [];
        var tasks = tokens
            .Select(IssueToken)
            .ToList();
        var returnee = (await Task.WhenAll(tasks))
            .ToList();
        return returnee;
    }
    public Task RecallToken(string tokenString);
    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<string> tokens);

    public string CreateTokenString(MalarkeyProfileToken profileToken);
    public string CreateTokenString(MalarkeyIdentityToken identityToken);


    public async Task<MalarkeyTokenValidationResult> ValidateToken(string token) => (await ValidateTokens([token])).First();


    public async Task<MalarkeyProfileAndIdentities?> ExtractProfileAndIdentities(HttpContext context)
    {
        var profileToken = await ValidateProfileToken(context);
        if (profileToken is MalarkeyTokenValidationSuccessResult profSucc)
        {
            if (profSucc.Token is MalarkeyProfileToken profTok)
            {
                var identityTokens = await ValidateIdentityTokens(context);
                var validIdentityTokens = identityTokens.Results
                    .Select(_ => _.Token)
                    .OfType<MalarkeyIdentityToken>()
                    .ToList();
                var identities = validIdentityTokens
                    .Select(_ => _.Identity)
                    .ToList();
                var returnee = new MalarkeyProfileAndIdentities(
                    Profile: profTok.Profile,
                    Identities: identities
                    );
                return returnee;
            }
        }
        return null;
    }
    public async Task<MalarkeyTokenValidationResult?> ValidateProfileToken(HttpContext context)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        if (!context.Request.Cookies.TryGetValue(MalarkeyConstants.Authentication.ProfileCookieName, out var profileCookie) || string.IsNullOrWhiteSpace(profileCookie))
            return null;
        return await ValidateToken(profileCookie);
    }

    public async Task<(IReadOnlyCollection<MalarkeyTokenValidationResult> Results, IReadOnlySet<string> FailedCookies)> ValidateIdentityTokens(HttpContext context)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var results = new List<MalarkeyTokenValidationResult>();
        var failedCookies = new HashSet<string>();
        var relevantCookies = context.Request.Cookies
            .Where(_ => _.Key.StartsWith(MalarkeyConstants.Authentication.IdentityCookieBaseName))
            .OrderBy(_ => _.Key)
            .ToList();
        var cookieValueMap = relevantCookies
            .ToDictionarySafe(_ => _.Value, _ => _.Key);
        var tokens = relevantCookies
            .Select(_ => _.Value)
            .ToList();
        if (!relevantCookies.Any())
            return (results, failedCookies);
        var returnee = await ValidateTokens(tokens.Select(tok => tok));
        var succeeded = returnee
            .OfType<MalarkeyTokenValidationSuccessResult>()
            .Select(_ => _.TokenString)
            .ToHashSet();
        var failed = cookieValueMap
            .Where(_ => !succeeded.Contains(_.Key))
            .Select(_ => _.Value)
            .ToHashSet();
        return (returnee, failed);
    }


    protected IServiceScopeFactory ServiceScopeFactory { get; }





}
