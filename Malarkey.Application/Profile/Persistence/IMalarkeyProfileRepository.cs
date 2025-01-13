using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Common;

namespace Malarkey.Application.Profile.Persistence;
public interface IMalarkeyProfileRepository
{
    protected Task<MalarkeyProfileAndIdentities?> LoadByProviderId(MalarkeyIdentityProvider provider, string providerId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByMicrosoft(string microsoftId) => 
        await LoadByProviderId(MalarkeyIdentityProvider.Microsoft, microsoftId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByGoogle(string googleId) =>
        await LoadByProviderId(MalarkeyIdentityProvider.Google, googleId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByFacebook(string facebookId) =>
        await LoadByProviderId(MalarkeyIdentityProvider.Facebook, facebookId);
    public async Task<MalarkeyProfileAndIdentities?> LoadBySpotify(string spotifyId) =>
        await LoadByProviderId(MalarkeyIdentityProvider.Spotify, spotifyId);

    Task SaveIdentityProviderToken(IdentityProviderToken token, Guid identityId); 

    Task<MalarkeyProfileAndIdentities?> CreateByIdentity(MalarkeyProfileIdentity identity);



    async Task<MalarkeyProfileAndIdentities?> LoadOrCreateByIdentity(MalarkeyProfileIdentity identity) => identity switch
    {
        MicrosoftIdentity micr => await LoadByMicrosoft(micr.MicrosoftId),
        GoogleIdentity goog => await LoadByGoogle(goog.GoogleId),
        FacebookIdentity fac => await LoadByFacebook(fac.FacebookId),
        SpotifyIdentity spot => await LoadBySpotify(spot.SpotifyId),
        _ => null
    } switch
    {
        null => await CreateByIdentity(identity),
        MalarkeyProfileAndIdentities profId => await SaveAndAddIdentityProviderToken(profId, identity.ProviderId, identity.IdentityProviderTokenToUse)
    };

    private async Task<MalarkeyProfileAndIdentities> SaveAndAddIdentityProviderToken(MalarkeyProfileAndIdentities ident, string idProviderId, IdentityProviderToken? token)
    {
        if (token == null)
            return ident;
        var relIdentity = ident.Identities
            .FirstOrDefault(_ => _.ProviderId == idProviderId);
        if (relIdentity == null) return ident;
        await SaveIdentityProviderToken(token, relIdentity.IdentityId);
        var returnee = ident with
        {
            Identities = ident.Identities
                .Select(iden => iden.IdentityId == relIdentity.IdentityId ? iden.WithToken(token) : iden)
                .ToList()
        };
        return returnee;
    }

    Task<ActionResult<MalarkeyProfile>> UpdateProfileName(Guid profileId, string name);
    Task<ActionResult<MalarkeyProfile>> UpdateFirstName(Guid profileId, string? firstName);
    Task<ActionResult<MalarkeyProfile>> UpdateLastName(Guid profileId, string? lastName);
    Task<ActionResult<MalarkeyProfile>> UpdatePrimaryEmail(Guid profileId, string? email);
    Task<ActionResult<MalarkeyProfile>> UpdateProfileImage(Guid profileId, byte[] image, string imageType);

    Task<MalarkeyProfileAndIdentities?> LoadProfileAndIdentities(Guid profileId);

}
