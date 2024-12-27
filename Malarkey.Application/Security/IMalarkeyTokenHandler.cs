using Malarkey.Application.Configuration;
using Malarkey.Application.Util;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Malarkey.Application.Security;
public interface IMalarkeyTokenHandler
{
    public string PublicKey { get; } 

    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey);
    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(MalarkeyProfileIdentity identity, string receiverPublicKey);
    public Task RecallToken(string tokenString);
    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<(string Token, string ReceiverPublicKey)> tokens);

    public string CreateTokenString(MalarkeyProfileToken profileToken, string receiverPublicKey);
    public string CreateTokenString(MalarkeyIdentityToken identityToken, string receiverPublicKey);


    public async Task<MalarkeyTokenValidationResult> ValidateToken(string token, string receiverPublicKey) => (await ValidateTokens([(token, receiverPublicKey)])).First();


    public async Task<MalarkeyProfileAndIdentities?> ExtractProfileAndIdentities(HttpContext context) => (await ValidateProfileToken(context)) switch
    {
        MalarkeyTokenValidationSuccessResult succ when succ.Token is MalarkeyProfileToken profTok => new MalarkeyProfileAndIdentities(
            Profile: profTok.Profile,
            Identities: (await ValidateIdentityTokens(context)).Results
               .Select(_ => _.Token)
               .OfType<MalarkeyIdentityToken>()
               .Select(_ => _.Identity)
               .ToList()

            ),
        _ => null
    };
    public async Task<MalarkeyTokenValidationResult?> ValidateProfileToken(HttpContext context)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        if(!context.Request.Cookies.TryGetValue(MalarkeyApplicationConstants.MalarkeyProfileCookieName, out var profileCookie) || string.IsNullOrWhiteSpace(profileCookie))
            return null;
        var publicKeyString = ExtractPublicKey(context, scope);
        if(publicKeyString == null)
            return new MalarkeyTokenValidationErrorResult(profileCookie, "No public key for validation");
        return await ValidateToken(profileCookie, publicKeyString);
    }

    public async Task<(IReadOnlyCollection<MalarkeyTokenValidationResult> Results, IReadOnlySet<string> FailedCookies)> ValidateIdentityTokens(HttpContext context)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var results = new List<MalarkeyTokenValidationResult>();
        var failedCookies = new HashSet<string>();
        var relevantCookies = context.Request.Cookies
            .Where(_ => _.Key.StartsWith(MalarkeyApplicationConstants.MalarkeyIdentityCookieBaseName))
            .OrderBy(_ => _.Key)
            .ToList();
        var cookieValueMap = relevantCookies
            .ToDictionarySafe(_ => _.Value, _ => _.Key);
        var tokens = relevantCookies
            .Select(_ => _.Value)
            .ToList();
        var publicKeyString = ExtractPublicKey(context, scope);
        if (!relevantCookies.Any() || publicKeyString == null)
            return (results, failedCookies);
        var returnee = await ValidateTokens(tokens.Select(tok => (tok, publicKeyString)));
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


    public async Task BakeCookies(HttpContext context, MalarkeyProfile profile, IReadOnlyCollection<MalarkeyProfileIdentity> identities)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var publicKeyString = ExtractPublicKey(context, scope);
        if(publicKeyString != null)
        {
            var (profTok, profTokString) = await IssueToken(profile, publicKeyString);
            context.Response.Cookies.Append(MalarkeyApplicationConstants.MalarkeyProfileCookieName, profTokString);
            foreach(var (iden, indx) in identities.OrderBy(_ => _.GetType().Name).Select((_,indx) => (_,indx)))
            {
                var (idenTok, idenTokString) = await IssueToken(iden, publicKeyString);
                context.Response.Cookies.Append($"{MalarkeyApplicationConstants.MalarkeyIdentityCookieBaseName}.{indx}", idenTokString);
            }
        }
    }

    public void BakeCookies(HttpContext context, MalarkeyProfileToken profile, IReadOnlyCollection<MalarkeyIdentityToken> identities)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var publicKeyString = ExtractPublicKey(context, scope);
        if (publicKeyString != null)
        {
            var profTokenString = CreateTokenString(profile, publicKeyString);
            context.Response.Cookies.Append(MalarkeyApplicationConstants.MalarkeyProfileCookieName, profTokenString);
            foreach (var (iden, indx) in identities.OrderBy(_ => _.GetType().Name).Select((_, indx) => (_, indx)))
            {
                var idenTokenString = CreateTokenString(iden, publicKeyString);
                context.Response.Cookies.Append($"{MalarkeyApplicationConstants.MalarkeyIdentityCookieBaseName}.{indx}", idenTokenString);
            }
        }
    }


    protected IServiceScopeFactory ServiceScopeFactory { get; }

    private string? ExtractPublicKey(HttpContext context, IServiceScope scope)
    {
        var publicKey = context.Connection.ClientCertificate?.PublicKey;
        if (publicKey == null)
        {
            var config = scope.ServiceProvider.GetRequiredService<IOptions<MalarkeyApplicationConfiguration>>().Value;
            publicKey = config.SigningCertificate.AsCertificate.PublicKey;
        }
        var returnee = publicKey.GetRSAPublicKey()?.ExportRSAPublicKeyPem()?.CleanCertificate();
        return returnee;
    }

}
