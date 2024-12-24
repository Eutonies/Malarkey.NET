using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyProfileAbsorbeeEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyProfileAbsorbeeDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyProfileAbsorbeeDbo> builder)
    {
        builder.ToTable("profile_absorbees");
        builder.HasNoKey();
        builder.Property(nameof(MalarkeyProfileAbsorbeeDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(MalarkeyProfileAbsorbeeDbo.Absorbee)).HasColumnName("absorbee");

    }
}
