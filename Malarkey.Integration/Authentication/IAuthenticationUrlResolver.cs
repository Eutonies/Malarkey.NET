using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Integration.Authentication;
public interface IAuthenticationUrlResolver
{
    string AuthenticateUrl { get; }

}
