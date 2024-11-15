using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyProfileAbsorberEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyProfileAbsorberDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyProfileAbsorberDbo> builder)
    {
        builder.ToTable("profile_absorbeer");
        builder.HasNoKey();
        builder.Property(nameof(MalarkeyProfileAbsorberDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(MalarkeyProfileAbsorberDbo.Absorber)).HasColumnName("absorber");

    }
}
