using Malarkey.Application.Profile;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application;
public static class DependencyInjectionApplication
{

    public static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IProfileService, ProfileService>();
        return builder;
    }


}
