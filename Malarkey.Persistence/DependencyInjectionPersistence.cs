using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using Malarkey.Persistence.Authentication;
using Malarkey.Persistence.Configuration;
using Malarkey.Persistence.Context;
using Malarkey.Persistence.Profile;
using Malarkey.Persistence.Token.Model;
using Malarkey.Security.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Application.Profile;
using Malarkey.Application.Authentication;

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
        builder.Services.AddSingleton<IMalarkeyAuthenticationSessionRepository, MalarkeyAuthenticationSessionRepository>();
        builder.Services.AddSingleton<IMalarkeyProfileRepository, MalarkeyProfileRepository>();
        builder.Services.AddSingleton<IMalarkeyTokenRepository, MalarkeyTokenRepository>();
        builder.Services.AddSingleton<IVerificationEmailHandler, VerificationEmailHandler>();
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
