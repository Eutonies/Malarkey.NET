using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Profile.Model;
using Malarkey.Persistence.Token.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using Malarkey.Application.Common;

namespace Malarkey.Persistence.Profile;
internal class MalarkeyProfileRepository : IMalarkeyProfileRepository
{
    private readonly IDbContextFactory<MalarkeyDbContext> _dbContextFactory;
    private SemaphoreSlim _nameUniqueLock = new SemaphoreSlim(1);
    private static readonly TimeSpan TimeBetweenVerificationEmails = TimeSpan.FromHours(2);

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
        profileInsertee = await SaveAndEnsureUniqueName(profileInsertee, cont);

        var insertee = new MalarkeyIdentityDbo
        {
            ProfileId = profileInsertee.ProfileId,
            IdentityName = identity.FirstName,
            MiddleNames = identity.MiddleNames,
            LastName = identity.LastName,
            Provider = provider,
            ProviderId = identity.ProviderId,
            PreferredName = identity.PreferredNameToUse,
            Email = identity.EmailToUse
        };
        cont.Identities.Add(insertee);
        await cont.SaveChangesAsync();
        var idProviderToken = identity.IdentityProviderTokenToUse;
        if (idProviderToken != null)
        {
            var tokenInsertee = idProviderToken.ToDbo(insertee.IdentityId);
            cont.Add(tokenInsertee);
            await cont.SaveChangesAsync();
            identity = insertee.ToDomain(tokenInsertee);
        }
        else
        {
            identity = insertee.ToDomain(null);
        }
        var profile = await ConvertWithEmailVerificationInfo(cont, profileInsertee);
        return new MalarkeyProfileAndIdentities(profile, [identity]);
    }

    public async Task<MalarkeyProfileAndIdentities?> LoadByProviderId(MalarkeyIdentityProvider provider, string providerId)
    {
        var providerDbo = provider.ToDbo();
        await using var cont = await _dbContextFactory.CreateDbContextAsync();
        var ident = await cont.Identities
            .Where(_ => _.Provider == providerDbo && _.ProviderId == providerId)
            .FirstOrDefaultAsync();
        if (ident == null)
            return null;
        var activeProfileId = await FindUnAbsorbedProfileId(cont, ident.ProfileId);
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
        var domainProfile = await ConvertWithEmailVerificationInfo(cont, profile);
        return new MalarkeyProfileAndIdentities(domainProfile, identities, Absorbees:  absorbees);
    }

    public async Task SaveIdentityProviderToken(IdentityProviderToken token, Guid identityId)
    {
        await using var cont = await _dbContextFactory.CreateDbContextAsync();
        var insertee = token.ToDbo(identityId);
        cont.Add(insertee);
        await cont.SaveChangesAsync();
    }

    public async Task<MalarkeyProfileAndIdentities> AttachIdentityToProfile(MalarkeyProfileIdentity identity, Guid profileId)
    {
        var asRepo = (IMalarkeyProfileRepository)this;
        var existing = await asRepo.LoadByIdentity(identity);
        var existingIdentity = existing?.Identities?
            .FirstOrDefault(id => id.IdentityProvider == identity.IdentityProvider && id.ProviderId == identity.ProviderId);
        if (existing != null && existing.Profile.ProfileId == profileId)
        {
            if (identity.IdentityProviderTokenToUse != null && existingIdentity != null)
            {
                await SaveIdentityProviderToken(identity.IdentityProviderTokenToUse, existingIdentity.IdentityId);
                var reloaded = await LoadProfileAndIdentities(profileId);
                return reloaded!;
            }
            return existing;
        }
        await using (var cont = await _dbContextFactory.CreateDbContextAsync()) {
            // Identity is attached to other profile
            if (existing != null)
            {
                var parentProfileId = await FindUnAbsorbedProfileId(cont, existing.Profile.ProfileId);
                var toUpdate = await cont.Profiles
                    .FirstAsync(_ => _.ProfileId == parentProfileId);
                toUpdate.AbsorbedBy = profileId;
                cont.Update(toUpdate);
                await cont.SaveChangesAsync();
            }
        }
        var returnee = await LoadProfileAndIdentities(profileId);
        return returnee!;

    }

    private async Task<MalarkeyProfileDbo> SaveAndEnsureUniqueName(MalarkeyProfileDbo profile, MalarkeyDbContext cont) 
    {
        await _nameUniqueLock.WaitAsync();
        try
        {
            var baseName = profile.ProfileName;
            var alreadyExists = await cont.Profiles
                .AnyAsync(_ => _.ProfileNameUniqueness == profile.ProfileName.ToLower());
            var random = new Random();
            while (alreadyExists)
            {
                profile.ProfileName = baseName + random.Next(1000, 1_000_000_000);
                alreadyExists = await cont.Profiles
                    .AnyAsync(_ => _.ProfileNameUniqueness == profile.ProfileName.ToLower());
            }
            profile.ProfileNameUniqueness = profile.ProfileName.ToLower();
            cont.Add(profile);
            await cont.SaveChangesAsync();
        }
        finally
        {
            _nameUniqueLock.Release();
        }
        return profile;
    }

    private MalarkeyIdentityProviderDbo ProviderFor(MalarkeyProfileIdentity ident) => ident switch
    {
        MicrosoftIdentity _ => MalarkeyIdentityProviderDbo.Microsoft,
        GoogleIdentity _ => MalarkeyIdentityProviderDbo.Google,
        FacebookIdentity _ => MalarkeyIdentityProviderDbo.Facebook,
        SpotifyIdentity _ => MalarkeyIdentityProviderDbo.Spotify,
        _ => MalarkeyIdentityProviderDbo.Facebook
    };

    public async Task<ActionResult<MalarkeyProfile>> UpdateProfileName(Guid profileId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new ErrorMessageActionResult<MalarkeyProfile>("Profile name cannot be null or empty");
        await _nameUniqueLock.WaitAsync();
        try
        {
            await using (var cont = await _dbContextFactory.CreateDbContextAsync())
            {
                var nameIsTaken = await cont.Profiles
                    .AnyAsync(_ => _.ProfileId != profileId && _.ProfileNameUniqueness == name.ToLower());
                if (nameIsTaken)
                    return new ErrorMessageActionResult<MalarkeyProfile>($"Name: {name} is already in use");
            }
            var returnee = await UpdateAndReturn(profileId, prof =>
            {
                prof.ProfileName = name; 
                prof.ProfileNameUniqueness = name.ToLower();
            });
            return returnee;
        }
        finally
        {
            _nameUniqueLock.Release();
        }
    }

    public Task<ActionResult<MalarkeyProfile>> UpdateFirstName(Guid profileId, string? firstName) => 
        UpdateAndReturn(profileId, prof => prof.FirstName = firstName);

    public Task<ActionResult<MalarkeyProfile>> UpdateLastName(Guid profileId, string? lastName) =>
        UpdateAndReturn(profileId, prof => prof.LastName = lastName);

    public Task<ActionResult<MalarkeyProfile>> UpdatePrimaryEmail(Guid profileId, string? email) =>
        UpdateAndReturn(profileId, prof =>
        {
            prof.PrimaryEmail = email;
        });


    public Task<ActionResult<MalarkeyProfile>> UpdateProfileImage(Guid profileId, byte[] image, string imageType) =>
        UpdateAndReturn(profileId, prof =>
        {
            prof.ProfileImage = image;
            prof.ProfileImageType = imageType;
        });

    private async Task<ActionResult<MalarkeyProfile>> UpdateAndReturn(Guid profileId, Action<MalarkeyProfileDbo> changer)
    {
        try
        {
            await using var cont = await _dbContextFactory.CreateDbContextAsync();
            var loaded = await cont.Profiles
                .FirstOrDefaultAsync(_ => _.ProfileId == profileId);
            if (loaded == null)
                return new ErrorMessageActionResult<MalarkeyProfile>($"Unable to load profile with ID: {profileId}");
            changer(loaded);
            cont.Update(loaded);
            await cont.SaveChangesAsync();
            var returnee = await ConvertWithEmailVerificationInfo(cont, loaded);
            return new SuccessActionResult<MalarkeyProfile>(returnee);
        }
        catch(Exception ex) 
        {
            return new ExceptionActionResult<MalarkeyProfile>(ex);
        }
    }

    public async Task<MalarkeyProfileAndIdentities?> LoadProfileAndIdentities(Guid profileId)
    {
        await using var cont = await _dbContextFactory.CreateDbContextAsync();
        var parentProfileId = await FindUnAbsorbedProfileId(cont, profileId);
        var profile = await cont.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(_ => _.ProfileId == parentProfileId);
        if(profile == null)
            return null;
        var profileIdQuery = cont.ProfileAbserbees
            .AsNoTracking()
            .Where(_ => _.ProfileId == parentProfileId)
            .Select(_ => _.Absorbee)
            .Union(
                cont.Profiles
                   .AsNoTracking()
                   .Where(_ => _.ProfileId == parentProfileId)
                   .Select(_ => _.ProfileId)
            );
        var identityQuery = from profId in profileIdQuery
                            join ident in cont.Identities
                            on profId equals ident.ProfileId
                            select ident;
        var identities = await identityQuery
            .AsNoTracking()
            .ToListAsync();
        var idProvTokenQuery = from tok in cont.IdentityProviderTokens.Where(_ => _.Expires > DateTime.Now)
                               join iden in identityQuery
                               on tok.IdentityId equals iden.IdentityId
                               select tok;
        var idProvTokenMap = (await idProvTokenQuery.ToListAsync())
            .ToDictionarySafe(_ => _.IdentityId);
        var domainIdentities = identities
            .Select(_ => _.ToDomain(idProvTokenMap.GetValueOrDefault(_.IdentityId)))
            .ToList();
        var absorbeeIds = await cont.ProfileAbserbees
            .Where(_ => _.ProfileId == profileId)
            .Select(_ => _.Absorbee)
            .ToListAsync();
        var domainProfile = await ConvertWithEmailVerificationInfo(cont, profile);
        var returnee = new MalarkeyProfileAndIdentities(domainProfile, domainIdentities, absorbeeIds);
        return returnee;
    }

    private static async Task<Guid> FindUnAbsorbedProfileId(MalarkeyDbContext cont,  Guid profileId)
    {
        var query = from absInfo in cont.ProfileAbserbees
                    join prof in cont.Profiles.Where(_ => _.AbsorbedBy == null)
                    on absInfo.Absorbee equals prof.ProfileId
                    select prof;
        var loaded = await query.AsNoTracking().FirstOrDefaultAsync();
        if (loaded == null)
            return profileId;
        return loaded.ProfileId;
    }


    private static async Task<MalarkeyProfile> ConvertWithEmailVerificationInfo(MalarkeyDbContext cont, MalarkeyProfileDbo dbo)
    {
        var emailIsVerified = false;
        DateTime? nextEmailVerificationTime = null;
        if(!string.IsNullOrWhiteSpace(dbo.PrimaryEmail))
        {
            var emailInfo = await cont.Emails
                .FirstOrDefaultAsync(_ => _.ProfileId == dbo.ProfileId && _.EmailAddress == dbo.PrimaryEmail.ToLower().Trim());
            if (emailInfo != null && emailInfo.VerifiedAt != null)
            {
                emailIsVerified = true;
            }
            else if(emailInfo != null)
            {
                nextEmailVerificationTime = emailInfo.LastVerificationMailSent?.Add(TimeBetweenVerificationEmails) ?? DateTime.Now.AddMinutes(-1);
            }
        }
        var returnee = dbo.ToDomain(emailIsVerified, nextEmailVerificationTime);
        return returnee;
    }


}
