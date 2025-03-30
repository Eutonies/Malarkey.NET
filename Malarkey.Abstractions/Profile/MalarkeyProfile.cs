using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;
public sealed record MalarkeyProfile(
    Guid ProfileId,
    string ProfileName,
    DateTime CreatedAt,
    Guid? AbsorbedBy,
    string? FirstName,
    string? LastName,
    string? PrimaryEmail,
    bool PrimaryEmailIsVerified,
    DateTime? NextVerificationSendTime,
    byte[]? ProfileImage = null,
    string? ProfileImageType = null
    )
{
    public static MalarkeyProfile With(string profileName, string? firstName = null, string? lastName = null, string? primaryEmail = null) => new MalarkeyProfile(
        ProfileId: Guid.NewGuid(),
        ProfileName: profileName,
        CreatedAt: DateTime.Now,
        AbsorbedBy: null,
        FirstName: firstName,
        LastName: lastName,
        PrimaryEmail: primaryEmail,
        PrimaryEmailIsVerified: false,
        NextVerificationSendTime: null);



}
