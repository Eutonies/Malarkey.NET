using Malarkey.API.Common;
using Malarkey.API.Profile.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Profile;
internal static class ProfileApi
{
    private const string DefaultAuthorizationPolicy = "";

    private static ApiEndpointGroup? _group;
    public static ApiEndpointGroup Group => _group ??= CreateGroup();

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
                )
            ]

        );


    private static readonly Delegate GetProfile = async (
        [FromServices] object Tessa

        ) =>
    {
        return new ProfileDto(Guid.Empty, "", "", "", []);
    };



}
