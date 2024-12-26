using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client;
public static class DependencyInjectionClient
{

    public static WebApplicationBuilder AddMalarkeyConfiguration(this WebApplicationBuilder builder)
    {

        return builder;
    }


}
