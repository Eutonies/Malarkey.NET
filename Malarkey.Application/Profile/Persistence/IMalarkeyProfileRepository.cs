using Malarkey.Domain.Authentication;
using Malarkey.Domain.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile.Persistence;
public interface IMalarkeyProfileRepository
{
    protected Task<MalarkeyProfileAndIdentities?> LoadByProviderId(MalarkeyOAuthIdentityProvider provider, string providerId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByMicrosoft(string microsoftId) => 
        await LoadByProviderId(MalarkeyOAuthIdentityProvider.Microsoft, microsoftId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByGoogle(string googleId) =>
        await LoadByProviderId(MalarkeyOAuthIdentityProvider.Google, googleId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByFacebook(string facebookId) =>
        await LoadByProviderId(MalarkeyOAuthIdentityProvider.Facebook, facebookId);
    public async Task<MalarkeyProfileAndIdentities?> LoadBySpotify(string spotifyId) =>
        await LoadByProviderId(MalarkeyOAuthIdentityProvider.Spotify, spotifyId);


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
