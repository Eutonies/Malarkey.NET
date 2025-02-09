using Malarkey.Application.Configuration;
using Malarkey.Application.Util;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Malarkey.Abstractions;

namespace Malarkey.Application.Security;
public interface IMalarkeyTokenHandler
{
    public string PublicKey { get; } 

    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey);
    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(MalarkeyProfileIdentity identity, string receiverPublicKey);

    public async Task<IReadOnlyCollection<(MalarkeyIdentityToken Token, string TokenString)>> IssueTokens(
        IEnumerable<MalarkeyProfileIdentity> tokens, 
        string audience)
    {
        if (!tokens.Any())
            return [];
        var tasks = tokens
            .Select(_ => IssueToken(_, audience))
            .ToList();
        var returnee = (await Task.WhenAll(tasks))
            .ToList();
        return returnee;
    }
    public Task RecallToken(string tokenString);
    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<(string Token, string ReceiverPublicKey)> tokens);

    public string CreateTokenString(MalarkeyProfileToken profileToken, string receiverPublicKey);
    public string CreateTokenString(MalarkeyIdentityToken identityToken, string receiverPublicKey);


    public async Task<MalarkeyTokenValidationResult> ValidateToken(string token, string receiverPublicKey) => (await ValidateTokens([(token, receiverPublicKey)])).First();


    public async Task<MalarkeyProfileAndIdentities?> ExtractProfileAndIdentities(HttpContext context, string receiver) 
    {
        var profileToken = await ValidateProfileToken(context, receiver);
        if(profileToken is MalarkeyTokenValidationSuccessResult profSucc)
        {
            if(profSucc.Token is MalarkeyProfileToken profTok)
            {
                var identityTokens = await ValidateIdentityTokens(context, receiver);
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
    public async Task<MalarkeyTokenValidationResult?> ValidateProfileToken(HttpContext context, string receiver)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        if(context.Request.Cookies.TryGetValue(MalarkeyConstants.Authentication.ProfileCookieName, out var profileCookie) && 
            !string.IsNullOrWhiteSpace(profileCookie)) 
            {
                return await ValidateToken(profileCookie, receiver);
            }
            return null;
    }

    public async Task<(IReadOnlyCollection<MalarkeyTokenValidationResult> Results, IReadOnlySet<string> FailedCookies)> ValidateIdentityTokens(HttpContext context, string receiver)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var results = new List<MalarkeyTokenValidationResult>();
        var failedCookies = new HashSet<string>();
        var relevantCookies = context.Request.Cookies
            .Where(_ => _.Key.StartsWith(MalarkeyConstants.Authentication.IdentityCookieBaseName))
            .Select(_ => (_.Key, Value: _.Value))
            .OrderBy(_ => _.Key)
            .ToList();
        var cookieValueMap = relevantCookies
            .ToDictionarySafe(_ => _.Value, _ => _.Key);
        var tokens = relevantCookies
            .Select(_ => _.Value)
            .ToList();
        if (!relevantCookies.Any())
            return (results, failedCookies);
        var returnee = await ValidateTokens(tokens.Select(tok => (tok, receiver)));
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
