using Microsoft.AspNetCore.Http;

namespace Malarkey.Server.Authentication;
public interface IMalarkeyServerAuthenticationCallbackHandler
{
    Task<IResult> HandleCallback(HttpRequest request);

}
