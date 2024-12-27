using Malarkey.Application.Configuration;
using Malarkey.Application.Profile;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Malarkey.Application;
public static class DependencyInjectionApplication
{

    public static WebApplicationBuilder AddApplicationConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<MalarkeyApplicationConfiguration>(builder.Configuration.GetSection(MalarkeyApplicationConfiguration.ConfigurationElementName));
        return builder;
    }


    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        return builder;
    }


}
