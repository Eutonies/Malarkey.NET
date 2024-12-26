using Malarkey.API.Common;
using Malarkey.API.Profile.Model;
using Malarkey.Application.Profile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Malarkey.Application.Common;
using Malarkey.Abstractions.Profile;
using Malarkey.Application.Security;

namespace Malarkey.API.Profile;
internal static class ProfileApi
{
    private const string DefaultAuthorizationPolicy = AuthorizationPolicies.IsAuthenticated;

    private static ApiEndpointGroup? _group;
    public static ApiEndpointGroup Group => _group ??= CreateGroup();
    private static string _sampleReceiver = RSA.Create(2048).ExportRSAPublicKeyPem();

    private static ApiEndpointGroup CreateGroup() => new ApiEndpointGroup(
        Name: "profile",
        AuthorizationPolicy: DefaultAuthorizationPolicy,
        Endpoints: [
            new ApiEndpoint(
                Name: "Certificate public key",
                Pattern: "certificate-public-key",
                Method: ApiHttpMethod.Get,
                Delegate: ([FromServices] IMalarkeyTokenHandler tokenHandler) => tokenHandler.PublicKey,
                Description: "Public key of certificate used to sign JWT's"
                ),
            new ApiEndpoint(
                Name: "sample-token",
                Pattern: "token",
                Method: ApiHttpMethod.Get,
                Delegate: GetSampleToken,
                Description: "creates a token"
                ),
            new ApiEndpoint(
                Name: "convert-token",
                Pattern: "token-to-profile",
                Method: ApiHttpMethod.Get,
                Delegate: ConvertToken,
                Description: "Converts token for sample purpose"
                )
            ]

        );


    private static readonly Delegate ConvertToken = async (
        [FromServices] IProfileService profileService,
        [FromQuery] string token
        ) => (await profileService.ExtractProfileFromToken(token, _sampleReceiver)) switch
        {
            SuccessActionResult<MalarkeyProfile> succ => (object) succ.Result,
            _ => "No can do Jackie-Boy!"
        };


    private static readonly Delegate GetSampleToken = async (
        [FromServices] IProfileService profileService
        ) =>
    {
        var token = await profileService.IssueSampleProfileToken(_sampleReceiver);
        return token switch
        {
            SuccessActionResult<string> succ => (object) succ.Result,
            _ => token
        };
        
    };




}
