using Malarkey.Abstractions.API.Profile;
using Malarkey.Abstractions.API.Profile.Requests;
using Malarkey.API.Common;
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


    [HttpPost("refresh-idprovider-token")]
    public Task<Results<BadRequest<string>, Ok<MalarkeyIdentityProviderTokenDto>>> RefreshIdentityProviderToken(
           [FromServices] 
           [FromBody] MalarkeyProfileRefreshProviderTokenRequest request) =>
        RequireClientCertificate(async clientCertificate => 
        {
            return new MalarkeyIdentityProviderTokenDto("", DateTime.Now, DateTime.Now);

        });


}
