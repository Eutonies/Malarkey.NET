using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyProfileEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyProfileDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyProfileDbo> builder)
    {
        builder.ToTable("profile");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileName)).HasColumnName("profile_name");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileNameUniqueness)).HasColumnName("profile_name_uniqueness");
        builder.Property(nameof(MalarkeyProfileDbo.CreatedAt)).HasColumnName("created_at");
        builder.Property(nameof(MalarkeyProfileDbo.AbsorbedBy)).HasColumnName("absorbed_by");
        builder.Property(nameof(MalarkeyProfileDbo.FirstName)).HasColumnName("first_name");
        builder.Property(nameof(MalarkeyProfileDbo.LastName)).HasColumnName("last_name");
        builder.Property(nameof(MalarkeyProfileDbo.PrimaryEmail)).HasColumnName("primary_email");
        builder.Property(nameof(MalarkeyProfileDbo.PrimaryEmailIsVerified)).HasColumnName("primary_email_is_verified");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileImage)).HasColumnName("profile_image");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileImage)).HasColumnType("bytea");
        builder.Property(nameof(MalarkeyProfileDbo.ProfileImageType)).HasColumnName("profile_image_type");


    }
}
