using Microsoft.AspNetCore.Http;

namespace Malarkey.Abstractions.Authentication;
public interface IMalarkeyAuthenticationCallbackHandler
{
    Task<IResult> HandleCallback(HttpRequest request);

}
