using Malarkey.Abstractions.API.Profile.Email;
using Malarkey.Application.Profile;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Profile.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile;
internal class VerificationEmailHandler : IVerificationEmailHandler
{

    private readonly IDbContextFactory<MalarkeyDbContext> _contextFactory;
    private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1);
    private readonly IServiceScopeFactory _scopeFactory;

    public VerificationEmailHandler(IDbContextFactory<MalarkeyDbContext> contextFactory, IServiceScopeFactory scopeFactory)
    {
        _contextFactory = contextFactory;
        _scopeFactory = scopeFactory;
    }

    public event EventHandler<VerifiableEmail> OnUpdate;

    public Task<VerifiableEmail?> EnsureEntryFor(string email, Guid profileId) => WithContext(async cont =>
    {
        if (!IsValidEmailAddress(email))
            return null;
        email = email
            .ToLower()
            .Trim();
        var returnee = await Locked(async () =>
        {
            var existing = await cont.Emails
                .FirstOrDefaultAsync(_ => _.EmailAddress == email && _.ProfileId == profileId);
            if (existing != null)
                return existing.ToDomain();
            var insertee = new VerifiableEmailDbo
            {
                EmailAddress = email,
                ProfileId = profileId,
                CodeString = Guid.NewGuid()
                  .ToString()
                  .Replace("-", "")
            };
            cont.Add(insertee);
            await cont.SaveChangesAsync();
            return insertee.ToDomain();
        });
        return returnee;
    });

    public bool IsValidEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        email = email
            .ToLower()
            .Trim();
        var isValid = true;
        try
        {
            var addr = new MailAddress(email);
            isValid = (addr.Address == email);
        }
        catch
        {
            isValid = false;
        }
        return isValid;
    }

    public Task<VerifiableEmail> RegisterVerification(long emailId, string codeString) => WithContext(async cont =>
    {
        var returnee = await Locked(async () =>
        {
            var loaded = await cont.Emails
               .FirstOrDefaultAsync(_ => _.EmailAddressId == emailId && _.CodeString == codeString);
            if (loaded == null)
                throw new Exception($"Did not find a verifiable email with ID: {emailId} and code string: {codeString}");
            if (loaded.VerifiedAt != null)
                return loaded.ToDomain();
            loaded.VerifiedAt = DateTime.Now;
            cont.Update(loaded);
            var profile = await cont.Profiles
               .FirstAsync(_ => _.ProfileId == loaded.ProfileId);
            if(profile.PrimaryEmail != null && profile.PrimaryEmail.ToLower().Trim() == loaded.EmailAddress.ToLower().Trim())
            {
                profile.PrimaryEmailIsVerified = true;
                cont.Update(profile);
            }
            await cont.SaveChangesAsync();
            return loaded.ToDomain();
        });
        OnUpdate?.Invoke(this, returnee);
        return returnee;
    });


    private async Task<T> WithContext<T>(Func<MalarkeyDbContext, Task<T>> toPerform)
    {
        await using var cont = await _contextFactory.CreateDbContextAsync();
        var returnee = await toPerform(cont);
        return returnee;
    }

    private async Task<T> Locked<T>(Func<Task<T>> toPerform)
    {
        await _updateLock.WaitAsync();
        try
        {
            return await toPerform();
        }
        finally
        {
            _updateLock.Release();
        }
    }

    public Task<VerifiableEmail?> SendVerificationMail(string email, Guid profileId) => WithContext(async cont =>
    {
        var entry = await EnsureEntryFor(email, profileId);
        if (entry == null)
            return null;
        var returnee = await Locked(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IVerificationEmailSender>();
            await emailSender.SendVerificationEmail(entry);
            var updatee = entry.ToDbo();
            updatee.LastVerificationMailSent = DateTime.Now;
            cont.Update(updatee);
            await cont.SaveChangesAsync();
            return updatee.ToDomain();
        });
        return returnee;
    });

    public Task<VerifiableEmail?> LoadEntryFor(string email, Guid profileId) => WithContext(async cont =>
    {
        if (!IsValidEmailAddress(email)) return null;
        email = email.ToLower().Trim();
        var loaded = await cont.Emails
            .FirstOrDefaultAsync(_ => _.EmailAddress == email && _.ProfileId == profileId);
        return loaded?.ToDomain();

    });
}
