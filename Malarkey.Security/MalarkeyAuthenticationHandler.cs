using Malarkey.Application;
using Malarkey.Application.Security;
using Malarkey.Domain.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Malarkey.Security;

internal class MalarkeyAuthenticationHandler : AuthenticationHandler<MalarkeyAuthenticationOptions>
{
    private IMalarkeyTokenHandler _tokenHandler;
    private ILogger<MalarkeyAuthenticationHandler> _logger;
    public MalarkeyAuthenticationHandler(
        IOptionsMonitor<MalarkeyAuthenticationOptions> options, 
        ILoggerFactory loggerFactory, 
        UrlEncoder encoder,
        IMalarkeyTokenHandler tokenHandler
        ) : base(options, loggerFactory, encoder)
    {
        _logger = loggerFactory.CreateLogger<MalarkeyAuthenticationHandler>();
        _tokenHandler = tokenHandler;

    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var context = Request.HttpContext;
        var profileValidationResult = await _tokenHandler.ValidateProfileToken(context);
        if (profileValidationResult == null)
            return AuthenticateResult.NoResult();
        if(profileValidationResult is MalarkeyTokenValidationSuccessResult succ && succ.ValidToken is MalarkeyProfileToken profTok)
        {
            var (identityValidations, failedCookieNames) = await _tokenHandler.ValidateIdentityTokens(context);
            var successIdentities = identityValidations
                .OfType<MalarkeyTokenValidationSuccessResult>()
                .Select(_ => _.Token)
                .OfType<MalarkeyIdentityToken>()
                .ToList();
            var principal = new MalarkeyClaimsPrincipal(profTok, successIdentities);
            var returnee = AuthenticateResult.Success(new AuthenticationTicket(principal, MalarkeyApplicationConstants.MalarkeyCookieSchemeName));
            return returnee;
        }
        return AuthenticateResult.NoResult();
    }
}
