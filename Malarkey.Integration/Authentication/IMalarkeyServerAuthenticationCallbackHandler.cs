using Microsoft.AspNetCore.Http;

namespace Malarkey.Integration.Authentication;
public interface IMalarkeyServerAuthenticationCallbackHandler
{
    Task HandleCallback(HttpRequest request);

}
