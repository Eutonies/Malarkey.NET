using Malarkey.Persistence.Authentication.Model;
using Malarkey.Persistence.Context.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model.Configurations;
internal class MalarkeyAuthenticationSessionParameterEntityConfiguration : MalarkeyEntityConfiguration<MalarkeyAuthenticationSessionParameterDbo>
{
    public override void Configure(EntityTypeBuilder<MalarkeyAuthenticationSessionParameterDbo> builder)
    {
        builder.ToTable("authentication_session_parameter");
        builder.HasKey(dbo => new { dbo.SessionId, dbo.ParameterNameUnique });
        builder.Property(nameof(MalarkeyAuthenticationSessionParameterDbo.SessionId)).HasColumnName("session_id");
        builder.Property(nameof(MalarkeyAuthenticationSessionParameterDbo.ParameterNameUnique)).HasColumnName("parameter_name_unique");
        builder.Property(nameof(MalarkeyAuthenticationSessionParameterDbo.ParameterName)).HasColumnName("parameter_name");
        builder.Property(nameof(MalarkeyAuthenticationSessionParameterDbo.ParameterValue)).HasColumnName("parameter_value");
    }
}
