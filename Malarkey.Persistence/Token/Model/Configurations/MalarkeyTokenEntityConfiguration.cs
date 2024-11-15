using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model.Configurations;
internal class MalarkeyTokenEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyTokenDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyTokenDbo> builder)
    {
        builder.ToTable("token");
        builder.Property(nameof(MalarkeyTokenDbo.ProfileId)).HasColumnName("token_id");
        builder.Property(nameof(MalarkeyTokenDbo.TokenType)).HasColumnName("token_type");
        builder.Property(nameof(MalarkeyTokenDbo.ProfileId)).HasColumnName("profile_id");
        builder.Property(nameof(MalarkeyTokenDbo.IdentityId)).HasColumnName("identity_id");
        builder.Property(nameof(MalarkeyTokenDbo.IssuedTo)).HasColumnName("issued_to");
        builder.Property(nameof(MalarkeyTokenDbo.IssuedAt)).HasColumnName("issued_at");
        builder.Property(nameof(MalarkeyTokenDbo.ValidUntil)).HasColumnName("valid_until");
        builder.Property(nameof(MalarkeyTokenDbo.RevokedAt)).HasColumnName("revoked_at");
    }
}
