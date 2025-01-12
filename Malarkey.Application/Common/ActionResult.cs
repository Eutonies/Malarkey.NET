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

public abstract record ErrorActionResult<TRes>
    : ActionResult<TRes>
{
    public string ErrorMessage { get; private set; }
    internal ErrorActionResult(
    string errorMessage
    )
    {
        ErrorMessage = errorMessage;
    }
}

public sealed record UnauthorizedActionResult<TRes>(
    string ErrorMessage
    ) : ErrorActionResult<TRes>(ErrorMessage);

public sealed record ExceptionActionResult<TRes>(
    Exception Exception
    ) : ErrorActionResult<TRes>(Exception.Message);

public sealed record ErrorMessageActionResult<TRes>(
    string ErrorMessage
    ) : ErrorActionResult<TRes>(ErrorMessage);
