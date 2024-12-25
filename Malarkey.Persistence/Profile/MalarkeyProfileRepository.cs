using Malarkey.Application.Profile.Persistence;
using Malarkey.Domain.Authentication;
using Malarkey.Domain.Profile;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Profile.Model;
using Malarkey.Persistence.Token.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile;
internal class MalarkeyProfileRepository : IMalarkeyProfileRepository
{
    private readonly IDbContextFactory<MalarkeyDbContext> _dbContextFactory;

    public MalarkeyProfileRepository(IDbContextFactory<MalarkeyDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<MalarkeyProfileAndIdentities?> CreateByIdentity(MalarkeyProfileIdentity identity)
    {
        await using var cont = await _dbContextFactory.CreateDbContextAsync();
        var provider = ProviderFor(identity);
        var existingIdent = await (cont.Identities
            .Where(_ => _.ProviderId == identity.ProviderId && _.Provider == provider))
            .FirstOrDefaultAsync();
        if (existingIdent != null)
            return null;

        var profileInsertee = new MalarkeyProfileDbo
        {
            CreatedAt = DateTime.Now,
            ProfileName = identity.FirstName
        };
        cont.Profiles.Add(profileInsertee);
        await cont.SaveChangesAsync();

        var insertee = new MalarkeyIdentityDbo
        {
            ProfileId = profileInsertee.ProfileId,
            IdentityName = identity.FirstName,
            MiddleNames = identity.MiddleNames,
            LastName = identity.LastName,
            Provider = provider,
            ProviderId = identity.ProviderId,
            PreferredName = (identity as MicrosoftIdentity)?.PreferredName,
            Email = (identity as SpotifyIdentity)?.Email
        };
        cont.Identities.Add(insertee);
        await cont.SaveChangesAsync();
        var idProviderToken = (identity as SpotifyIdentity)?.AccessToken;
        if (idProviderToken != null)
        {
            var tokenInsertee = new IdentityProviderTokenDbo
            {
                IdentityId = insertee.IdentityId,
                TokenString = idProviderToken.Token,
                Issued = idProviderToken.Issued,
                Expires = idProviderToken.Expires,
                RefreshToken = idProviderToken.RefreshToken
            };
            cont.Add(tokenInsertee);
            await cont.SaveChangesAsync();
            identity = insertee.ToDomain(tokenInsertee);
        }
        else
        {
            identity = insertee.ToDomain(null);
        }

        var profile = profileInsertee.ToDomain();
        return new MalarkeyProfileAndIdentities(profile, [identity]);
    }

    public async Task<MalarkeyProfileAndIdentities?> LoadByProviderId(MalarkeyOAuthIdentityProvider provider, string providerId)
    {
        var providerDbo = provider.ToDbo();
        await using var cont = await _dbContextFactory.CreateDbContextAsync();
        var ident = await cont.Identities
            .Where(_ => _.Provider == providerDbo && _.ProviderId == providerId)
            .FirstOrDefaultAsync();
        if (ident == null)
            return null;
        var activeProfileId = (await cont.ProfileAbsorbers
            .Where(_ => _.ProfileId == ident.ProfileId)
            .Select(_ => _.Absorber)
            .FirstOrDefaultAsync()) ?? ident.ProfileId;
        var profile = await cont.Profiles
            .Where(_ => _.ProfileId == activeProfileId)
            .FirstOrDefaultAsync();
        if(profile == null) return null;

        var activeAccessToken = await cont.IdentityProviderTokens
            .Where(_ => _.IdentityId == ident.IdentityId && _.Expires > DateTime.Now)
            .OrderByDescending(_ => _.Issued)
            .FirstOrDefaultAsync();

        var identities = (await cont.Identities
            .Where(_ => _.ProfileId == activeProfileId)
            .ToListAsync())
            .Select(_ => _.ToDomain(activeAccessToken))
            .ToList();
        var absorbees = await cont.ProfileAbserbees
            .Where(_ => _.ProfileId == activeProfileId)
            .Select(_ => _.Absorbee)
            .ToListAsync();
        return new MalarkeyProfileAndIdentities(profile.ToDomain(), identities, Absorbees:  absorbees);
            



    }


    private MalarkeyIdentityProviderDbo ProviderFor(MalarkeyProfileIdentity ident) => ident switch
    {
        MicrosoftIdentity _ => MalarkeyIdentityProviderDbo.Microsoft,
        GoogleIdentity _ => MalarkeyIdentityProviderDbo.Google,
        FacebookIdentity _ => MalarkeyIdentityProviderDbo.Facebook,
        SpotifyIdentity _ => MalarkeyIdentityProviderDbo.Spotify,
        _ => MalarkeyIdentityProviderDbo.Facebook
    };


}
