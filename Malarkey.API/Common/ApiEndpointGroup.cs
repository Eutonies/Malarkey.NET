using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Common;
public record ApiEndpointGroup(
    string Name,
    string? AuthorizationPolicy,
    IReadOnlyCollection<ApiEndpoint> Endpoints
    );
