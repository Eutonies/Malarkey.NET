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
internal class MalarkeyAuthenticationIdpSessionEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyAuthenticationIdpSessionDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyAuthenticationIdpSessionDbo> builder)
    {
        builder.ToTable("authentication_idp_session");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.IdpSessionId)).HasColumnName("idp_session_id");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.SessionId)).HasColumnName("session_id");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.IdProvider)).HasColumnName("id_provider");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.Nonce)).HasColumnName("nonce");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.CodeChallenge)).HasColumnName("code_challenge");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.CodeVerifier)).HasColumnName("code_verifier");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.InitTime)).HasColumnName("init_time");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.AuthenticatedTime)).HasColumnName("authenticated_time");
        builder.Property(nameof(MalarkeyAuthenticationIdpSessionDbo.Scopes)).HasColumnName("scopes");
    }
}
