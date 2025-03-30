using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Server.Authentication;
public interface IMalarkeyServerAuthenticationEventHandler
{
    event EventHandler<MalarkeyProfileIdentity> OnIdentificationRegistrationCompleted;

    void RegisterIdentificationCompleted(MalarkeyProfileIdentity identity);

}
