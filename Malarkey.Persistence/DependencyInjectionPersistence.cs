using Malarkey.Persistence.Configuration;
using Malarkey.Persistence.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence;
public static class DependencyInjectionPersistence
{
    public static WebApplicationBuilder AddPersistenceConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<PersistenceConfiguration>(builder.Configuration.GetSection(PersistenceConfiguration.ConfigurationElementName));
        return builder;
    }

    public static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<MalarkeyDbContext>(ConfigureDb, lifetime: ServiceLifetime.Singleton);
        builder.Services.AddDbContextFactory<MalarkeyDbContext>(ConfigureDb, lifetime: ServiceLifetime.Scoped);
        return builder;
    }

    private static void ConfigureDb(IServiceProvider services, DbContextOptionsBuilder builder)
    {
        var connectionString = services.GetRequiredService<IOptions<PersistenceConfiguration>>().Value.Db.ConnectionString;
        builder
            .UseNpgsql(connectionString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    }

}
