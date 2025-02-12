using Microsoft.AspNetCore.Http;

namespace Malarkey.Integration.Authentication;
public interface IMalarkeyServerAuthenticationCallbackHandler
{
    Task<IResult> HandleCallback(HttpRequest request);

}
