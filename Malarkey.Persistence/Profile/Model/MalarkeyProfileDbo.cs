using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class MalarkeyProfileDbo
{
    [Key]
    public Guid ProfileId { get; set; }
    public string ProfileName { get; set; }
    public string ProfileNameUniqueness { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? AbsorbedBy { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PrimaryEmail { get; set; }
    public bool PrimaryEmailIsVerified { get; set; }
    public byte[]? ProfileImage { get; set; }
    public string? ProfileImageType { get; set; }

    public MalarkeyProfile ToDomain() => new MalarkeyProfile(
        ProfileId,
        ProfileName,
        CreatedAt,
        AbsorbedBy,
        FirstName,
        LastName,
        PrimaryEmail,
        PrimaryEmailIsVerified,
        ProfileImage,
        ProfileImageType
        );


}
