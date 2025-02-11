using Malarkey.Abstractions;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Common;

[Route(MalarkeyConstants.API.ApiPath +  "/[controller]")]
public class MalarkeyController : ControllerBase
{


    protected string? ExtractCertificate() => Request.Headers
            .Where(_ => _.Key == MalarkeyConstants.API.ClientCertificateHeaderName)
            .Select(_ => _.Value.ToString().UrlDecoded())
            .FirstOrDefault();

    protected async Task<Results<BadRequest<string>,Ok<A>>> RequireClientCertificate<A>(Func<string, Task<A>> toPerform)
    {
        var clientCert = ExtractCertificate();
        if(clientCert == null)
        {
            return TypedResults.BadRequest($"No client certificate found in header: {MalarkeyConstants.API.ClientCertificateHeaderName}");
        }
        try
        {
            var result = await toPerform(clientCert);
            return TypedResults.Ok(result);
        }
        catch(Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }


}
