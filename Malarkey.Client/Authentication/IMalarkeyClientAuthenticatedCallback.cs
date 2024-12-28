using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
internal interface IMalarkeyClientAuthenticatedCallback
{
    Task<IResult> HandleCallback(HttpRequest request);

}
