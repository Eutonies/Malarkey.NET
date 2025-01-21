using Malarkey.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Malarkey.Abstractions.Profile;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationEvents : IMalarkeyServerAuthenticationEventHandler
{
    public event EventHandler<(MalarkeyProfileIdentity Identity, string State)> OnIdentificationRegistrationCompleted;

    public void RegisterIdentificationCompleted(MalarkeyProfileIdentity identity, string state)
    {
        OnIdentificationRegistrationCompleted?.Invoke(this,(identity, state));
    }
}
