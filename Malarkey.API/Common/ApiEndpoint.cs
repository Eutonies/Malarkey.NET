using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Common;
public record ApiEndpoint(
    string Name, 
    string Pattern, 
    ApiHttpMethod Method, 
    Delegate Delegate, 
    string Description, 
    string? AuthorizationPolicy = null
    );
