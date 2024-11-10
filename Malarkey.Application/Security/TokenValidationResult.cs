using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
internal abstract record TokenValidationResult(MalarkeyToken Token);

internal sealed record TokenValidationSuccessResult(MalarkeyToken ValidToken) : TokenValidationResult(ValidToken);

internal sealed record TokenValidationErrorResult(string ErrorMessage, MalarkeyToken InvalidToken): TokenValidationResult(InvalidToken);

