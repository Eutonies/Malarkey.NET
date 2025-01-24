using Malarkey.Abstractions;
using Malarkey.Abstractions.API.Profile;
using Malarkey.Abstractions.API.Profile.Requests;
using Malarkey.API.Common;
using Malarkey.Application.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Profile;
public class ProfileController : MalarkeyController
{


    [HttpPost(MalarkeyConstants.API.Paths.Profile.RefreshTokenRelativePath)]
    public Task<Results<BadRequest<string>, Ok<MalarkeyIdentityProviderTokenDto>>> RefreshIdentityProviderToken(
           [FromServices] IMalarkeyAuthenticationSessionRepository sessionHandler,
           [FromBody] MalarkeyProfileRefreshProviderTokenRequest request) =>
        RequireClientCertificate(async clientCertificate => 
        {
            var refreshed = await sessionHandler.Refresh(request.AccessToken, clientCertificate);
            if (refreshed == null)
                throw new Exception("Twas not possible to refresh token");
            return refreshed.ToDto();

        });


}
