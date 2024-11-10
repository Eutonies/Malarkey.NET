using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Common;
public abstract record ActionResult<TRes>
{
    internal ActionResult() { }
}

public sealed record SuccessActionResult<TRes>(
    TRes Result
    ) : ActionResult<TRes>;

public abstract record ErrorActionResult
    : ActionResult<object?>
{
    public string ErrorMessage { get; private set; }
    internal ErrorActionResult(
    string errorMessage
    )
    {
        ErrorMessage = errorMessage;
    }
}

public sealed record UnauthorizedActionResult(
    string ErrorMessage
    ) : ErrorActionResult(ErrorMessage);

public sealed record ExceptionActionResult(
    Exception Exception
    ) : ErrorActionResult(Exception.Message);
