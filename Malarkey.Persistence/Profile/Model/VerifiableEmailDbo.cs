using Malarkey.Abstractions.API.Profile.Email;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class VerifiableEmailDbo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long EmailAddressId { get; set; }
    public Guid ProfileId { get; set; }
    public string EmailAddress { get; set; }
    public string CodeString { get; set; }
    public DateTime? LastVerificationMailSent { get; set; }

    public DateTime? VerifiedAt { get; set; }


    public VerifiableEmail ToDomain() => new VerifiableEmail(
        EmailAddressId: EmailAddressId,
        ProfileId: ProfileId,
        EmailAddress: EmailAddress,
        CodeString: CodeString,
        LastVerificationEmailSent: LastVerificationMailSent,
        VerifiedAt: VerifiedAt
        );



}


internal static class VerifiableEmailDboExtensions
{
    public static VerifiableEmailDbo ToDbo(this VerifiableEmail em) => new VerifiableEmailDbo
    {
        EmailAddressId = em.EmailAddressId,
        ProfileId = em.ProfileId,
        EmailAddress = em.EmailAddress.Trim().ToLower(),
        CodeString = em.CodeString,
        LastVerificationMailSent = em.LastVerificationEmailSent,
        VerifiedAt = em.VerifiedAt

    };
}