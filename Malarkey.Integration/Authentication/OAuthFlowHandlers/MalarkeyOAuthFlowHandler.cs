using Malarkey.Domain.Authentication;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal abstract class MalarkeyOAuthFlowHandler : IMalarkeyOAuthFlowHandler
{
    protected readonly MalarkeyOAuthNamingScheme _namingScheme;
    protected readonly MalarkeyIdentityProviderConfiguration _conf;
    protected readonly MalarkeyIntegrationConfiguration _intConf;

    public abstract MalarkeyOAuthIdentityProvider HandlerFor { get; }

    protected virtual string[] DefaultScopes => ["openid"];
    protected virtual string DefaultResponseType => "code";
    protected virtual string DefaultCodeChallengeMethod => "S256";

    public MalarkeyOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf)
    {
        _intConf = intConf.Value;
        _conf = ProduceConfiguration();
        _namingScheme = ProduceNamingScheme();
    }
    public abstract string AuthorizationEndpoint { get; }

    protected abstract MalarkeyOAuthNamingScheme ProduceNamingScheme();
    protected abstract MalarkeyIdentityProviderConfiguration ProduceConfiguration();


    public virtual RedirectHttpResult ProduceRedirect(MalarkeyAuthenticationSession session)
    {
        var url = ProduceAuthorizationUrl(session);
        var res = TypedResults.Redirect(url);
        return res;
    }



    public virtual IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session)
    {
        var returnee = new Dictionary<string, string>();
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ResponseType] = _conf.ResponseType!;
        returnee[_namingScheme.ResponseMode] = _conf.ResponseMode ?? "form_post";
        returnee[_namingScheme.RedirectUri] = _intConf.RedirectUrl;
        returnee[_namingScheme.Scope] = (_conf.Scopes ?? DefaultScopes)
            .MakeString()
            .UrlEncoded();
        if (session.Nonce != null)
            returnee[_namingScheme.Nonce] = session.Nonce;
        returnee[_namingScheme.CodeChallenge] = session.CodeChallenge;
        returnee[_namingScheme.CodeChallengeMethod] = DefaultCodeChallengeMethod;
        returnee[_namingScheme.State] = session.State;
        return returnee;
    }
    public virtual string ProduceAuthorizationRequestQueryString(IReadOnlyDictionary<string, string> queryParameters) => queryParameters
        .Select(p => $"{p.Key}={p.Value}")
        .MakeString("&");



    public virtual string ProduceAuthorizationUrl(MalarkeyAuthenticationSession session)
    {
        var queryParameters = ProduceRequestQueryParameters(session);
        var queryString = ProduceAuthorizationRequestQueryString(queryParameters);
        var baseUrl = AuthorizationEndpoint;
        var returnee = $"{baseUrl}?{queryString}";
        return returnee;
    }

    public abstract Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, IMalarkeyOAuthFlowHandler.RedirectData redirectData);

    public abstract Task<IMalarkeyOAuthFlowHandler.RedirectData?> ExtractRedirectData(HttpRequest request);

}


