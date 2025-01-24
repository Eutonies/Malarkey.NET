using Azure.Core;
using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParDefs = Malarkey.Abstractions.MalarkeyConstants.AuthenticationRequestQueryParameters;

namespace Malarkey.Integration.Authentication;
public static class MalarkeyServerAuthenticationSessionResolver
{

    private static readonly string SendToLookup = ParDefs.SendToName.ToLower();
    private static readonly string IdProviderLookup = ParDefs.IdProviderName.ToLower();
    private static readonly string SendToStateLookup = ParDefs.SendToStateName.ToLower();
    private static readonly string ScopesLookup = ParDefs.ScopesName.ToLower();
    private static readonly string ExistingProfileIdLookup = ParDefs.ExistingProfileIdName.ToLower();
    private static HashSet<string> NamedParameters = new List<string>
    {
        SendToLookup,
        IdProviderLookup, 
        SendToStateLookup,
        ScopesLookup,
        ExistingProfileIdLookup
    }.ToHashSet();

    public static MalarkeyAuthenticationSession ResolveSession(this HttpRequest req, string defaultAudience)
    {
        var queryPars = req.Query
            .Select(pair => new MalarkeyAuthenticationSessionParameter(
                SessionId: 0L,
                Name: pair.Key,
                Value: pair.Value.ToString()
            ))
            .GroupBy(_ => _.NameKey)
            .ToDictionarySafe(_ => _.Key, grp => grp.First() with { Value = grp.Select(_ => _.Value).Order().MakeString(" ")});
        var requestedSendTo = queryPars
            .GetValueOrDefault(SendToLookup)?.Value;
        var sendTo = requestedSendTo ?? $"/{MalarkeyConstants.Authentication.ServerAuthenticationPath}";
        var isInternal = !sendTo.ToLower().StartsWith("http");
        var requestedIdProviderString = queryPars
            .GetValueOrDefault(IdProviderLookup)?.Value;
        var requestedIdProvider = requestedIdProviderString?
            .Pipe(_ => Enum.Parse<MalarkeyIdentityProvider>(_));
        var requestState = queryPars
            .GetValueOrDefault(SendToStateLookup)?.Value;
        var requestedScopes = queryPars
            .GetValueOrDefault(ScopesLookup)?.Value?.Split(" ");
        var audience = req.Headers.TryGetValue(MalarkeyConstants.Authentication.AudienceHeaderName, out var pubKey) ?
              pubKey.ToString() :
              defaultAudience;
        var existingProfileId = queryPars
            .TryGetValue(ExistingProfileIdLookup, out var profId) ? 
               ((Guid?) Guid.Parse(profId.Value)) : 
               null;

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
              RequestParameters: requestParameters,
              IdpSession: null);
        return returnee;

    }

}
