using Malarkey.Domain.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile.Persistence;
public interface IMalarkeyProfileRepository
{
    protected Task<MalarkeyProfileAndIdentities?> LoadByProviderId(MalarkeyIdentityProviderDbo provider, string providerId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByMicrosoft(string microsoftId) => 
        await LoadByProviderId(MalarkeyIdentityProviderDbo.Microsoft, microsoftId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByGoogle(string googleId) =>
        await LoadByProviderId(MalarkeyIdentityProviderDbo.Google, googleId);

    public async Task<MalarkeyProfileAndIdentities?> LoadByFacebook(string facebookId) =>
        await LoadByProviderId(MalarkeyIdentityProviderDbo.Facebook, facebookId);


    Task<MalarkeyProfileAndIdentities?> CreateByIdentity(ProfileIdentity identity);

    async Task<MalarkeyProfileAndIdentities?> LoadOrCreateByIdentity(ProfileIdentity identity) => identity switch
    {
        MicrosoftIdentity micr => await LoadByMicrosoft(micr.MicrosoftId),
        GoogleIdentity goog => await LoadByGoogle(goog.GoogleId),
        FacebookIdentity fac => await LoadByFacebook(fac.FacebookId),
        _ => null
    } switch
    {
        null => await CreateByIdentity(identity),
        MalarkeyProfileAndIdentities profId => profId
    };


}
