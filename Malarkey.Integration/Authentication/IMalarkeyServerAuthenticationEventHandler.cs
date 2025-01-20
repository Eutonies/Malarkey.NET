using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public interface IMalarkeyServerAuthenticationEventHandler
{
    event EventHandler<(MalarkeyProfileIdentity Identity, string State)> OnIdentificationRegistrationCompleted;


}
