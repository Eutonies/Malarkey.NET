using Malarkey.Application.Profile.Persistence;
using Malarkey.Application.Util;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeyMicrosoftOAuthFlowHandler : MalarkeyOAuthFlowHandler
{
    
    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Microsoft;


    public MalarkeyMicrosoftOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf) : base(intConf)
    {
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate.Replace("{tenant}", _conf.Tenant);

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeyMicrosoftOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Microsoft;
    public override async Task<IMalarkeyOAuthFlowHandler.RedirectData?> ExtractRedirectData(HttpRequest request)
    {
        var body = await request.Body.ReadAsStringAsync();
        var keyValued = ParseUrlValuePairs(body);
        if (!keyValued.TryGetValue("state", out var state))
            return null;
        var returnee = new IMalarkeyOAuthFlowHandler.RedirectData(
            State: state,
            Token: keyValued.GetValueOrDefault("token"),
            IdToken: keyValued.GetValueOrDefault("id_token")
        );
        return returnee;
    }

    public override async Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, IMalarkeyOAuthFlowHandler.RedirectData redirectData)
    {
        await Task.CompletedTask;
        if (redirectData.IdToken == null)
            return null;
        var jwtHandler = new JwtSecurityTokenHandler();
        var parsedToken = jwtHandler.ReadJwtToken(redirectData.IdToken);
        var claimsMap = parsedToken.Claims
            .Select(_ => (_.Type, _.Value))
            .ToDictionarySafe(_ => _.Type, _ => _.Value);
        if(!claimsMap.TryGetValue("nonce", out var nonce))
            return null;
        var sessionNonce = session?.IdpSession?.Nonce?.UrlDecoded();
        if (sessionNonce != null && sessionNonce != nonce)
            return null;
        if (!claimsMap.TryGetValue("oid", out var userId))
            return null;
        userId = ShortenUserObjectId(userId);
        if (!claimsMap.TryGetValue("email", out var email))
            return null;
        if (!claimsMap.TryGetValue("preferred_username", out var prefName))
            prefName = "unknown";
        if (!claimsMap.TryGetValue("given_name", out var givenName))
            givenName = "unknown";
        if (!claimsMap.TryGetValue("family_name", out var famName))
            famName = "unknown";

        var identity = new MicrosoftIdentity(
            IdentityId: Guid.Empty,
            ProfileId: Guid.Empty,
            MicrosoftId: userId,
            PreferredName: prefName,
            Name: givenName,
            MiddleNames: null,
            LastName: famName
            );
        return identity;
    }



    private readonly static Regex UserObjectIdRegex = new Regex("^0*(.*)");
    private static string ShortenUserObjectId(string userObjectId)
    {
        userObjectId = userObjectId.Replace("-", "");
        var parseResult = UserObjectIdRegex.Match(userObjectId);
        if (parseResult.Groups.Count <= 1)
            return userObjectId;
        var returnee = parseResult.Groups[1].Value;
        return returnee;
    }




}

