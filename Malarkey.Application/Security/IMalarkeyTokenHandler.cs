using Malarkey.Application.Configuration;
using Malarkey.Application.Util;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Malarkey.Application.Security;
public interface IMalarkeyTokenHandler
{
    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey);
    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(ProfileIdentity identity, string receiverPublicKey);
    public Task RecallToken(string tokenString);
    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<(string Token, string ReceiverPublicKey)> tokens);
    public async Task<MalarkeyTokenValidationResult> ValidateToken(string token, string receiverPublicKey) => (await ValidateTokens([(token, receiverPublicKey)])).First();
    public async Task<MalarkeyTokenValidationResult> ValidateHttpContext(HttpContext context)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        if(!context.Request.Cookies.TryGetValue(MalarkeyApplicationConstants.MalarkeyProfileCookieName, out var profileCookie) || string.IsNullOrWhiteSpace(profileCookie))
            return new MalarkeyTokenValidationErrorResult("Failed to locate profile cookie");
        var publicKeyString = ExtractPublicKey(context, scope);
        if(publicKeyString == null)
            return new MalarkeyTokenValidationErrorResult("No public key for validation");
        return await ValidateToken(profileCookie, publicKeyString);
    }

    public async Task BakeCookies(HttpContext context, MalarkeyProfile profile, IReadOnlyCollection<ProfileIdentity> identities)
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

    IServiceScopeFactory ServiceScopeFactory { get; }

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
