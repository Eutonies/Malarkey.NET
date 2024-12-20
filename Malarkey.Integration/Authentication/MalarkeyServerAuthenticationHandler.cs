using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Malarkey.Security;
using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Malarkey.Integration.Authentication;
public class MalarkeyServerAuthenticationHandler : AuthenticationHandler<MalarkeyServerAuthenticationHandlerOptions>
{
    private readonly IMalarkeyTokenHandler _tokenHandler;
    private readonly IMalarkeyAuthenticationSessionHandler _sessionHandler;

    /// <summary>
    /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
    /// If it is not provided a default instance is supplied which does nothing when the methods are called.
    /// </summary>
    protected new MalarketAuthenticationEvents Events
    {
        get { return (MalarketAuthenticationEvents) base.Events!; }
        set { base.Events = value; }
    }

    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyServerAuthenticationHandlerOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder,
        IMalarkeyTokenHandler tokenHandler,
        IMalarkeyAuthenticationSessionHandler sessionHandler
        ) : base(options, logger, encoder)
    {
        _tokenHandler = tokenHandler;
        _sessionHandler = sessionHandler;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() => (await _tokenHandler.ExtractProfileAndIdentities(Context)) switch
        {
            null => AuthenticateResult.Fail("No profile info found"),
            MalarkeyProfileAndIdentities p => AuthenticateResult.Success(new AuthenticationTicket(
                principal: p.Profile.ToClaimsPrincipal(p.Identities),
                IntegrationConstants.MalarkeyAuthenticationScheme
                ))
        };



    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var requestedUrl = OriginalPath;
        var state = 

    }





}
