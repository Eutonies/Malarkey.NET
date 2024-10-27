namespace Malarkey.Service;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("apsettings.json");
        builder.Configuration.AddJsonFile("apssettings.local.json", optional: true);
        return builder;
    }


}
