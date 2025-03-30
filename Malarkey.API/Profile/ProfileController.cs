using Malarkey.Abstractions;
using Malarkey.Abstractions.API.Profile;
using Malarkey.Abstractions.API.Profile.Requests;
using Malarkey.Abstractions.Authentication;
using Malarkey.API.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.API.Profile;
public class ProfileController : MalarkeyController
{


    [HttpPost(MalarkeyConstants.API.Paths.Profile.RefreshTokenRelativePath)]
    public Task<Results<BadRequest<string>, Ok<MalarkeyIdentityProviderTokenDto>>> RefreshIdentityProviderToken(
           [FromServices] IMalarkeyAuthenticationSessionCache sessionRepo,
           [FromServices] IServiceProvider serviceProvider,
           [FromBody] MalarkeyProfileRefreshProviderTokenRequest request) =>
        RequireClientCertificate(async clientCertificate => 
        {
            var identityProvider = request.IdentityProvider.ToDomain();
            var refreshToken = await sessionRepo.LoadRefreshTokenForAccessToken(request.AccessToken, clientCertificate);
            var refresher = serviceProvider.GetRequiredKeyedService<IMalarkeyIdentityProviderTokenRefresher>(identityProvider);
            var refreshed = await refresher.Refresh(request.AccessToken, clientCertificate);
            if (refreshed == null)
                throw new Exception("Twas not possible to refresh token");
            refreshed = await sessionRepo.UpdateIdentityProviderToken(refreshed);
            return refreshed.ToDto();
        });


}
