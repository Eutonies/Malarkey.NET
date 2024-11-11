using Malarkey.API.Common;
using Malarkey.API.Profile.Model;
using Malarkey.Application.Profile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;


using System.Text;
using System.Threading.Tasks;
using Malarkey.Application.Common;
using Malarkey.Domain.Profile;

namespace Malarkey.API.Profile;
internal static class ProfileApi
{
    private const string DefaultAuthorizationPolicy = "";

    private static ApiEndpointGroup? _group;
    public static ApiEndpointGroup Group => _group ??= CreateGroup();
    private static string _sampleReceiver = RSA.Create(2048).ExportRSAPublicKeyPem();

    private static ApiEndpointGroup CreateGroup() => new ApiEndpointGroup(
        Name: "profile",
        AuthorizationPolicy: DefaultAuthorizationPolicy,
        Endpoints: [
            new ApiEndpoint(
                Name: "get-profile",
                Pattern: "profile",
                Method: ApiHttpMethod.Get,
                Delegate: GetProfile,
                Description: "Loads profile"
                ),
            new ApiEndpoint(
                Name: "sample-token",
                Pattern: "token",
                Method: ApiHttpMethod.Get,
                Delegate: GetSampleToken,
                Description: "creates a token"
                )
            ]

        );


    private static readonly Delegate GetProfile = async (
        [FromServices] object Tessa

        ) =>
    {
        return new ProfileDto(Guid.Empty, "", "", "", []);
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
