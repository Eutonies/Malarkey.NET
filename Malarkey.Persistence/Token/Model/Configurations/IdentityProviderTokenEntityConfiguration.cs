using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Token.Model.Configurations;
internal class IdentityProviderTokenEntityConfiguration : MalarkeyEntityConfiguration<IdentityProviderTokenDbo>
{
    public override void Configure(EntityTypeBuilder<IdentityProviderTokenDbo> builder)
    {
        builder.ToTable("id_provider_token");
        builder.Property(nameof(IdentityProviderTokenDbo.IdProviderTokenId)).HasColumnName("token_id");
        builder.Property(nameof(IdentityProviderTokenDbo.IdentityId)).HasColumnName("identity_id");
        builder.Property(nameof(IdentityProviderTokenDbo.TokenString)).HasColumnName("token_string");
        builder.Property(nameof(IdentityProviderTokenDbo.Issued)).HasColumnName("issued_at");
        builder.Property(nameof(IdentityProviderTokenDbo.Expires)).HasColumnName("expires_at");
        builder.Property(nameof(IdentityProviderTokenDbo.RefreshToken)).HasColumnName("refresh_token");
    }
}
