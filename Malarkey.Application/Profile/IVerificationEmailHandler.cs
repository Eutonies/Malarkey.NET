using Malarkey.Abstractions.API.Profile.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile;
public interface IVerificationEmailHandler
{
    event EventHandler<VerifiableEmail> OnUpdate;
    bool IsValidEmailAddress(string email);
    Task<VerifiableEmail?> LoadEntryFor(string email, Guid profileId);
    Task<VerifiableEmail?> EnsureEntryFor(string email, Guid profileId);
    Task<VerifiableEmail> RegisterVerification(long emailId, string codeString);

    Task<VerifiableEmail?> SendVerificationMail(string email, Guid profileId);

}
