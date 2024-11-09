using Malarkey.API.Common;
using Malarkey.API.Profile;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API;
public static class DependencyInjectionApi
{

    public static WebApplication UseApi(this WebApplication app)
    {
        app
            .Map(ProfileApi.Group);

        return app;
    }

}
