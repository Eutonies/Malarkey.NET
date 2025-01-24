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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal abstract class MalarkeyOAuthFlowHandler : IMalarkeyOAuthFlowHandler
{
    protected static readonly char[] CodeVerifierAllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~".ToCharArray();

    protected readonly MalarkeyOAuthNamingScheme _namingScheme;
    protected readonly MalarkeyIdentityProviderConfiguration _conf;
    protected readonly MalarkeyIntegrationConfiguration _intConf;

    public abstract MalarkeyIdentityProvider HandlerFor { get; }

    protected virtual string[] DefaultScopes => ["openid"];
    protected virtual string DefaultResponseType => "code";
    protected virtual string DefaultCodeChallengeMethod => "S256";
    protected virtual int NumberOfCharsInCodeVerifier => 43;

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
        returnee[_namingScheme.Scope] = (session.Scopes ?? (_conf.Scopes ?? DefaultScopes))
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

    protected IReadOnlyDictionary<string, string> ParseUrlValuePairs(string input)
    {
        var splitted = input.Split('&')
            .ToList();
        var keyValued = splitted
            .Where(_ => _.Contains('='))
            .Select(_ => _.Split('='))
            .Select(_ => (Key: _[0], Value: _[1]))
            .ToDictionarySafe(_ => _.Key, _ => _.Value);
        return keyValued;
    }

    public virtual MalarkeyAuthenticationIdpSession PopulateIdpSession(MalarkeyAuthenticationSession session)
    {
        var (verifier, challenge) = GenerateChallengeAndVerifier();
        var returnee = new MalarkeyAuthenticationIdpSession(
            IdpSessionId: 0L,
            SessionId: session.SessionId,
            IdProvider: HandlerFor,
            Nonce: GenerateNonce(),
            CodeChallenge: challenge,
            CodeVerifier: verifier,
            InitTime: DateTime.Now,
            AuthenticatedTime: null,
            Scopes: ScopesFor(session)
            );
        return returnee;
    }

    protected virtual string[] ScopesFor(MalarkeyAuthenticationSession session) => session.RequestedScopes ?? DefaultScopes;

    protected virtual (string Verifier, string Challenge) GenerateChallengeAndVerifier()
    {
        using var random = RandomNumberGenerator.Create();
        var randomBytes = new byte[NumberOfCharsInCodeVerifier];
        random.GetBytes(randomBytes);
        randomBytes = randomBytes
            .Select(_ => (byte)(_ % CodeVerifierAllowedChars.Length))
            .ToArray();
        var verifier = randomBytes
            .Select(byt => CodeVerifierAllowedChars[byt])
            .MakeString("");
        var verifierBytes = UTF8Encoding.UTF8.GetBytes(verifier);
        var challengeBytes = SHA256.HashData(verifierBytes);
        var challenge = Convert.ToBase64String(challengeBytes).Substring(0, NumberOfCharsInCodeVerifier);
        return (verifier, challenge);
    }

    protected virtual string GenerateNonce()
    {
        using var random = RandomNumberGenerator.Create();
        var randomBytes = new byte[32];
        random.GetBytes(randomBytes);
        var returnee = Convert.ToBase64String(randomBytes);
        return returnee;
    }



}


