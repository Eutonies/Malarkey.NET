using Malarkey.Persistence.Profile.Model;
using Malarkey.Persistence.Token.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Context;
internal class MalarkeyDbContext : DbContext
{

    public DbSet<MalarkeyProfileDbo> Profiles { get; set; }
    public DbSet<MalarkeyProfileAbsorbeeDbo> ProfileAbserbees { get; set; }
    public DbSet<MalarkeyProfileAbsorberDbo> ProfileAbsorbers { get; set; }

    public DbSet<MalarkeyIdentityDbo> Identities { get; set; }
    public DbSet<MalarkeyTokenDbo> Tokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MalarkeyDbContext).Assembly);
    }


}
