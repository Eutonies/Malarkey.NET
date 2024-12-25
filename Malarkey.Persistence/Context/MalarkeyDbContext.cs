using Malarkey.Domain.Authentication;
using Malarkey.Persistence.Authentication;
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


    public DbSet<MalarkeyAuthenticationSessionDbo> AuthenticationSessions { get; set; }
    public DbSet<IdentityProviderTokenDbo> IdentityProviderTokens { get; set; }

    public MalarkeyDbContext(DbContextOptions<MalarkeyDbContext> opts) : base(opts)
    {


    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MalarkeyDbContext).Assembly);
    }


}
