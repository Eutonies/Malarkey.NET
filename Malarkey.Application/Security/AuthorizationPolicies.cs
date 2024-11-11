using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
public static class AuthorizationPolicies
{
    public const string IsAuthenticated = "IsAuthenticated";

    public static void RegisterIsAuthenticatedPolicy(this AuthorizationOptions opts) =>
        opts.AddPolicy(IsAuthenticated, builder => builder.RequireAssertion(cont => true));


}

