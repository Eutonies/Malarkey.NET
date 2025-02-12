﻿using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ParDefs = Malarkey.Abstractions.MalarkeyConstants.AuthenticationRequestQueryParameters;

namespace Malarkey.Abstractions.Authentication;
public static class MalarkeyAuthenticationSessionResolver
{

    private static readonly string SendToLookup = ParDefs.SendToName.ToLower();
    private static readonly string IdProviderLookup = ParDefs.IdProviderName.ToLower();
    private static readonly string SendToStateLookup = ParDefs.SendToStateName.ToLower();
    private static readonly string ScopesLookup = ParDefs.ScopesName.ToLower();
    private static readonly string ExistingProfileIdLookup = ParDefs.ExistingProfileIdName.ToLower();
    private static readonly string AlwaysChallengeLookup = ParDefs.AlwaysChallengeName.ToLower();
    private static readonly string EncryptedStateLookup = ParDefs.EncryptedStateName.ToLower();
    private static readonly string ClientPublicKeyLookup = ParDefs.ClientPublicKey.ToLower();

    private static HashSet<string> NamedParameters = new List<string>
    {
        SendToLookup,
        IdProviderLookup, 
        SendToStateLookup,
        ScopesLookup,
        ExistingProfileIdLookup,
        AlwaysChallengeLookup
    }.ToHashSet();

    public static MalarkeyAuthenticationSession ResolveSession(
        this HttpRequest req, 
        string defaultAudience,
        string? sendToOverride = null,
        string? sendToStateOverride = null,
        string? idProviderOverride = null,
        string? scopesOverride = null,
        string? profileIdOverride = null,
        string? alwaysChallengeOverride = null,
        string? encryptedStateOverride = null,
        string? clientPublicKeyOverride = null,
        RSA? malarkeyPrivateKey = null
        )
    {
        var queryPars = req.Query
            .Select(pair => new MalarkeyAuthenticationSessionParameter(
                SessionId: 0L,
                Name: pair.Key,
                Value: pair.Value.ToString()
            ))
            .GroupBy(_ => _.NameKey)
            .ToDictionarySafe(_ => _.Key, grp => grp.First() with { Value = grp.Select(_ => _.Value).Order().MakeString(" ")});
        var requestedSendTo = sendToOverride ?? queryPars
            .GetValueOrDefault(SendToLookup)?.Value;
        if (requestedSendTo == null)
            requestedSendTo = req.Path;
        var sendTo = requestedSendTo ?? $"/{MalarkeyConstants.Authentication.ServerAuthenticationPath}";
        var isInternal = !sendTo.ToLower().StartsWith("http");
        var requestedIdProviderString = idProviderOverride ?? queryPars
            .GetValueOrDefault(IdProviderLookup)?.Value;
        var requestedIdProvider = requestedIdProviderString?
            .Pipe(_ => Enum.Parse<MalarkeyIdentityProvider>(_));
        var requestState = sendToStateOverride ?? queryPars
            .GetValueOrDefault(SendToStateLookup)?.Value;
        var encryptedState = encryptedStateOverride ?? queryPars
            .GetValueOrDefault(EncryptedStateLookup)?.Value;
        var encryptState = false;
        if(encryptedState != null && malarkeyPrivateKey != null)
        {
            encryptState = true;
            var encedBytes = Convert.FromBase64String(encryptedState);
            var bytes = malarkeyPrivateKey.Decrypt(encedBytes, MalarkeyConstants.RSAPadding);
            requestState = UTF8Encoding.UTF8.GetString(bytes);
        }
        var requestedScopes = (scopesOverride ?? queryPars
            .GetValueOrDefault(ScopesLookup)?.Value)?.Split(" ");
        var audience = clientPublicKeyOverride?.CleanCertificate() ?? defaultAudience;
        if (queryPars.TryGetValue(ClientPublicKeyLookup, out var clientCertPar))
            audience = clientCertPar.Value.CleanCertificate();
        var existingProfileId = profileIdOverride?.Pipe(Guid.Parse) ?? (queryPars
            .TryGetValue(ExistingProfileIdLookup, out var profId) ? 
               ((Guid?) Guid.Parse(profId.Value)) : 
               null);

        var alwaysChallenge = alwaysChallengeOverride?.Pipe(_ => _.ToLower().Contains("true")) ?? ( queryPars
            .TryGetValue(AlwaysChallengeLookup, out var allwChal) ?
            allwChal.Value.ToLower().Contains("true") :
            false);

        var requestParameters = queryPars
            .Where(_ => !NamedParameters.Contains(_.Key))
            .Select(_ => _.Value)
            .ToList();
        var returnee = new MalarkeyAuthenticationSession(
              SessionId: 0L,
              State: Guid.Empty.ToString(),
              IsInternal: isInternal,
              InitTime: DateTime.Now,
              SendTo: sendTo,
              RequestedSendTo: requestedSendTo,
              RequestedIdProvider: requestedIdProvider,
              RequestState: requestState,
              RequestedScopes: requestedScopes,
              AuthenticatedTime: null,
              ProfileTokenId: null,
              IdentityTokenId: null,
              Audience: audience,
              ExistingProfileId: existingProfileId,
              AlwaysChallenge: alwaysChallenge,
              RequestParameters: requestParameters,
              IdpSession: null,
              EncryptState: encryptState);
        return returnee;
    }

    public static MalarkeyAuthenticationParameters ResolveParameters(this HttpRequest request) => request
        .ResolveSession("dummy")
        .ToParameters();


}
