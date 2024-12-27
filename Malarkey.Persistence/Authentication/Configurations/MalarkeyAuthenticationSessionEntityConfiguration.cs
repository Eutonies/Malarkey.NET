using Malarkey.Persistence.Authentication;
using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyAuthenticationSessionEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyAuthenticationSessionDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyAuthenticationSessionDbo> builder)
    {
        builder.ToTable("authentication_session");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.SessionId)).HasColumnName("session_id");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.State)).HasColumnName("state");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.IdProvider)).HasColumnName("id_provider");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.Nonce)).HasColumnName("nonce");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.Forwarder)).HasColumnName("forwarder");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.CodeChallenge)).HasColumnName("code_challenge");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.CodeVerifier)).HasColumnName("code_verifier");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.InitTime)).HasColumnName("init_time");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.AuthenticatedTime)).HasColumnName("authenticated_time");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.ProfileTokenId)).HasColumnName("profile_token_id");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.IdentityTokenId)).HasColumnName("identity_token_id");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.Audience)).HasColumnName("audience");
        builder.Property(nameof(MalarkeyAuthenticationSessionDbo.Scopes)).HasColumnName("scopes");
    }
}
