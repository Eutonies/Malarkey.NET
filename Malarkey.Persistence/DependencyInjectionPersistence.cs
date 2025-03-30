using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using Malarkey.Persistence.Authentication;
using Malarkey.Persistence.Configuration;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Profile;
using Malarkey.Persistence.Token.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.NameTranslation;
using Malarkey.Application.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Application.Cleanup;
using Malarkey.Persistence.Cleanup;

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
        //builder.Services.AddDbContext<MalarkeyDbContext>(ConfigureDb, contextLifetime : ServiceLifetime.Scoped);
        builder.Services.AddSingleton<IMalarkeyAuthenticationSessionCache, MalarkeyAuthenticationSessionRepository>();
        builder.Services.AddSingleton<IMalarkeyProfileRepository, MalarkeyProfileRepository>();
        builder.Services.AddSingleton<IMalarkeyTokenCache, MalarkeyTokenRepository>();
        builder.Services.AddSingleton<IVerificationEmailHandler, VerificationEmailHandler>();
        builder.Services.AddSingleton<IPersistenceCleaner, PersistenceCleaner>();
        return builder;
    }

    private static void ConfigureDb(IServiceProvider services, DbContextOptionsBuilder builder)
    {
        var connectionString = services.GetRequiredService<IOptions<PersistenceConfiguration>>().Value.Db.ConnectionString;
        builder
            .UseNpgsql(connectionString, opts =>
            {
                opts.MapEnum<MalarkeyIdentityProviderDbo>("provider_type", nameTranslator: new NpgsqlNullNameTranslator());
                opts.MapEnum<MalarkeyTokenTypeDbo>("token_type", nameTranslator: new NpgsqlNullNameTranslator());

            })
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    }

}
