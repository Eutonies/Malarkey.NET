using Malarkey.Domain.Authentication;
using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Token;

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
        MalarkeyProfileAndIdentities profId => profId
    };


}
