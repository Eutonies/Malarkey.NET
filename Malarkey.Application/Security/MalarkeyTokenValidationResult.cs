using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
internal abstract record MalarkeyTokenValidationResult(MalarkeyToken? Token = null);

internal sealed record MalarkeyTokenValidationSuccessResult(MalarkeyToken ValidToken) : MalarkeyTokenValidationResult(ValidToken);
internal sealed record MalarkeyTokenValidationExceptionResult(Exception Exception) : MalarkeyTokenValidationResult();

internal sealed record MalarkeyTokenValidationErrorResult(string ErrorMessage): MalarkeyTokenValidationResult();

