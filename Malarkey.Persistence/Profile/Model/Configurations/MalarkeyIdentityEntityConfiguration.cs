using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyIdentityEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyIdentityDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyIdentityDbo> builder)
    {
        builder.ToTable("profile_identity");
        builder.Property(nameof(MalarkeyIdentityDbo.IdentityId)).HasColumnName("identity_id");
        builder.Property(nameof(MalarkeyIdentityDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(MalarkeyIdentityDbo.Provider)).HasColumnName("provider");
        builder.Property(nameof(MalarkeyIdentityDbo.ProviderId)).HasColumnName("provider_id");
        builder.Property(nameof(MalarkeyIdentityDbo.IdentityName)).HasColumnName("identity_name");
        builder.Property(nameof(MalarkeyIdentityDbo.PreferredName)).HasColumnName("preferred_name");
        builder.Property(nameof(MalarkeyIdentityDbo.MiddleNames)).HasColumnName("middle_names");
        builder.Property(nameof(MalarkeyIdentityDbo.LastName)).HasColumnName("last_name");
        builder.Property(nameof(MalarkeyIdentityDbo.Email)).HasColumnName("email");

    }
}
