using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Context.Configurations;
internal abstract class MalarkeyEntityConfiguration<TEnt> : IEntityTypeConfiguration<TEnt> where TEnt : class
{
    public abstract void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEnt> builder);
}
