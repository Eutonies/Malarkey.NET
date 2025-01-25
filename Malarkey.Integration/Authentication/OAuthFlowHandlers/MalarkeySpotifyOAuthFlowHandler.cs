using Malarkey.Application.Util;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.Naming;
using Malarkey.Integration.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SpyOff.Infrastructure.Tracks;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Malarkey.Abstractions.Token;
using static Malarkey.Integration.Authentication.OAuthFlowHandlers.IMalarkeyOAuthFlowHandler;
using Malarkey.Application.Authentication;

namespace Malarkey.Integration.Authentication.OAuthFlowHandlers;
internal class MalarkeySpotifyOAuthFlowHandler : MalarkeyOAuthFlowHandler, IMalarkeyIdentityProviderTokenRefresher
{
    private readonly IHttpClientFactory _httpClientFactory;
    public override MalarkeyIdentityProvider HandlerFor => MalarkeyIdentityProvider.Spotify;

    public MalarkeySpotifyOAuthFlowHandler(IOptions<MalarkeyIntegrationConfiguration> intConf, IHttpClientFactory httpClientFactory) : base(intConf)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override string AuthorizationEndpoint => _conf.AuthorizationEndpointTemplate;

    protected override MalarkeyOAuthNamingScheme ProduceNamingScheme() => MalarkeySpotifyOAuthNamingScheme.Init(_conf.NamingSchemeOverwrites);
    protected override MalarkeyIdentityProviderConfiguration ProduceConfiguration() => _intConf.Spotify;

    public override IReadOnlyDictionary<string, string> ProduceRequestQueryParameters(MalarkeyAuthenticationSession session, MalarkeyAuthenticationIdpSession idpSession)
    {
        var returnee = new Dictionary<string, string>();
        returnee[_namingScheme.ClientId] = _conf.ClientId;
        returnee[_namingScheme.ResponseType] = _conf.ResponseType!;
        returnee[_namingScheme.RedirectUri] = _intConf.RedirectUrl;
        returnee[_namingScheme.Scope] = idpSession.Scopes
            .MakeString(" ")
            .UrlEncoded();
        returnee[_namingScheme.CodeChallenge] = idpSession.CodeChallenge
            .Replace('+','-')
            .Replace('/','_');
        returnee[_namingScheme.CodeChallengeMethod] = DefaultCodeChallengeMethod;
        returnee[_namingScheme.State] = session.State;
        return returnee;
    }

    public override async Task<RedirectData?> ExtractRedirectData(HttpRequest request)
    {
        var keyValued = request.Query
            .Select(_ => (_.Key, Value: _.Value.ToString()))
            .ToDictionarySafe(_ => _.Key,  _ => _.Value);
        if (!keyValued.TryGetValue("code", out var code))
            return null;
        if(!keyValued.TryGetValue("state", out var state))
            return null;
        await Task.CompletedTask;
        var returnee = new RedirectData(
            State: state,
            Code: code
            );
        return returnee;
    }

    public override async Task<MalarkeyProfileIdentity?> ResolveIdentity(MalarkeyAuthenticationSession session, RedirectData redirectData)
    {
        if (redirectData.Code == null)
            return null;
        var url = _conf.TokenEndpointTemplate!;
        var codeVerifier = session.IdpSession!.CodeVerifier;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formParameters = new List<(string, string)>
        {
            ("grant_type", "authorization_code"),
            ("code", redirectData.Code!),
            ("redirect_uri", _intConf.RedirectUrl),
            ("client_id", _conf.ClientId),
            ("code_verifier", codeVerifier)
        };
        request.Content = formParameters.ToFormContent();
        IdentityProviderToken? accessToken = null;
        using (var client = _httpClientFactory.CreateClient())
        {
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return null;
            }
            accessToken = await ParseAsToken(response);
        }
        if (accessToken == null)
            return null;
        var authHeader = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        using var apiHttpClient = _httpClientFactory.CreateClient();
        apiHttpClient.DefaultRequestHeaders.Authorization = authHeader;
        apiHttpClient.BaseAddress = new Uri(_conf.ApiBaseUrl!);
        var apiClient = new SpotifyApiClient(apiHttpClient);
        var userInfo = await apiClient.GetCurrentUsersProfileAsync();
        var returnee = new SpotifyIdentity(
            IdentityId: Guid.Empty,
            ProfileId: Guid.Empty,
            SpotifyId: userInfo.Id,
            Name: userInfo.Display_name,
            MiddleNames: null,
            LastName: null,
            Email: userInfo.Email,
            AccessToken: accessToken
        );

        return returnee;
    }

    private static async Task<AccessTokenResponse> Parse(HttpResponseMessage response) => (await response.Content.ReadFromJsonAsync<AccessTokenResponseJsonLayout>())
        .Pipe(resp => new AccessTokenResponse(
            Token: resp!.access_token,
            Scopes: resp.scope.Split(" "),
            Expires: DateTime.Now.AddSeconds(resp.expires_in),
            RefreshToken: resp.refresh_token
    ));

    private static async Task<IdentityProviderToken> ParseAsToken(HttpResponseMessage response)
    {
        var tokenResponse = await Parse(response);
        var accessToken = tokenResponse.Token;
        var accessTokenIssued = DateTime.Now;
        var accessTokenExpires = tokenResponse.Expires;
        var refreshToken = tokenResponse.RefreshToken;
        var accessTokenScopes = tokenResponse.Scopes;
        var returnee = new IdentityProviderToken(Token: accessToken, Issued: accessTokenIssued, Expires: accessTokenExpires, RefreshToken: refreshToken, Scopes: accessTokenScopes);
        return returnee;
    }




    public async Task<IdentityProviderToken?> Refresh(string refreshToken)
    {
        var url = _conf.TokenEndpointTemplate!;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formParameters = new List<(string, string)>
        {
            ("grant_type", "refresh_token"),
            ("refresh_token", refreshToken),
            ("client_id", _conf.ClientId)
        };
        request.Content = formParameters.ToFormContent();
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", $"{_conf.ClientId}:{_conf.ClientSecret}");
        using var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return null;
        var returnee = await ParseAsToken(response);
        return returnee;
    }

    public override async Task<IdentityProviderToken?> Refresh(string refreshToken, string audiencePublicKey)
    {
        var url = _conf.TokenEndpointTemplate!;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formParameters = new List<(string, string)>
        {
            ("grant_type", "refresh_token"),
            ("refresh_token", refreshToken),
            ("client_id", _conf.ClientId)
        };
        request.Content = formParameters.ToFormContent();
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", $"{_conf.ClientId}:{_conf.ClientSecret}");
        using var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return null;
        var returnee = await ParseAsToken(response);
        return returnee;
    }

    private record AccessTokenResponse(
        string Token,
        string[] Scopes,
        DateTime Expires,
        string RefreshToken
        ); 

    private record AccessTokenResponseJsonLayout(
        string access_token,
        string scope,
        int expires_in,
        string refresh_token
        );


}
