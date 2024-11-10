using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace Malarkey.Security;
public static class DependencyInjectionSecurity
{

    public static WebApplicationBuilder AddSecurity(this WebApplicationBuilder builder)
    {


        return builder;
    }

}
