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



}
