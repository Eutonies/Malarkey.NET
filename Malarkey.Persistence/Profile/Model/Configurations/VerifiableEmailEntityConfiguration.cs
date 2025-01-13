using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class VerifiableEmailEntityConfiguration : MalarkeyEntityConfiguration<VerifiableEmailDbo>
{
    public override void Configure(EntityTypeBuilder<VerifiableEmailDbo> builder)
    {
        builder.ToTable("profile_email");
        builder.Property(nameof(VerifiableEmailDbo.EmailAddressId)).HasColumnName("email_id");
        builder.Property(nameof(VerifiableEmailDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(VerifiableEmailDbo.CodeString)).HasColumnName("code_string");
        builder.Property(nameof(VerifiableEmailDbo.EmailAddress)).HasColumnName("email_address");
        builder.Property(nameof(VerifiableEmailDbo.LastVerificationMailSent)).HasColumnName("last_send_time");
        builder.Property(nameof(VerifiableEmailDbo.VerifiedAt)).HasColumnName("verified_at");
    }
}
