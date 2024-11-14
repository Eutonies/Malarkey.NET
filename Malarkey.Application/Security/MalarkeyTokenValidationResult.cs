using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
public abstract record MalarkeyTokenValidationResult(MalarkeyToken? Token = null);

public sealed record MalarkeyTokenValidationSuccessResult(MalarkeyToken ValidToken) : MalarkeyTokenValidationResult(ValidToken);
public sealed record MalarkeyTokenValidationExceptionResult(Exception Exception) : MalarkeyTokenValidationResult();

public sealed record MalarkeyTokenValidationErrorResult(string ErrorMessage): MalarkeyTokenValidationResult();

