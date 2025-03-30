using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public abstract record MalarkeyTokenValidationResult(string TokenString, MalarkeyToken? Token = null);

public sealed record MalarkeyTokenValidationSuccessResult(string TokenString, MalarkeyToken ValidToken) : MalarkeyTokenValidationResult(TokenString, ValidToken);
public sealed record MalarkeyTokenValidationExceptionResult(string TokenString, Exception Exception) : MalarkeyTokenValidationResult(TokenString);

public sealed record MalarkeyTokenValidationErrorResult(string TokenString, string ErrorMessage): MalarkeyTokenValidationResult(TokenString);

