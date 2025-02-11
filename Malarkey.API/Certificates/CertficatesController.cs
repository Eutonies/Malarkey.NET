using Malarkey.Abstractions;
using Malarkey.API.Common;
using Malarkey.Application.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.API.Certificates;
[Route(MalarkeyConstants.API.ApiPath + "/certificates")]
public class CertficatesController : MalarkeyController
{

    [HttpGet(MalarkeyConstants.API.Paths.Certificates.SigningCertificateRelativePath)]
    public async Task<Results<BadRequest<string>, Ok<string>>> GetSigningCertificate(
        [FromServices] IOptions<MalarkeyApplicationConfiguration> appConf
        )
    {
        await Task.CompletedTask;
        var certificate = appConf.Value.SigningCertificate.ExportableCertificate;
        return TypedResults.Ok(certificate);
    }

    [HttpGet(MalarkeyConstants.API.Paths.Certificates.HostingCertificateRelativePath)]
    public async Task<Results<BadRequest<string>, Ok<string>>> GetHostingCertificate(
        [FromServices] IOptions<MalarkeyApplicationConfiguration> appConf
        )
    {
        await Task.CompletedTask;
        var certificate = appConf.Value.HostingCertificate.ExportableCertificate;
        return TypedResults.Ok(certificate);
    }

}
