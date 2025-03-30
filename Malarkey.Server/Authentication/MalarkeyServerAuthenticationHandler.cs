using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Malarkey.Integration.Authentication.OAuthFlowHandlers;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Integration.Configuration;
using Malarkey.Abstractions.Token;
using Malarkey.Integration.Authentication;

namespace Malarkey.Server.Authentication;
public class MalarkeyServerAuthenticationHandler : MalarkeyIntegrationAuthenticationHandler
{
    private readonly IMalarkeyProfileRepository _profileRepo;
    private readonly IMalarkeyServerAuthenticationEventHandler _eventHandler;
    public MalarkeyServerAuthenticationHandler(
        IOptionsMonitor<MalarkeyIntegrationAuthenticationHandlerOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        MalarkeySynchronizer synchronizer,
        IMalarkeyTokenIssuer tokenIssuer,
        IMalarkeyAuthenticationSessionCache sessionCache,
        IEnumerable<IMalarkeyOAuthFlowHandler> flowHandlers,
        IMalarkeyProfileRepository profileRepo,
        IOptions<MalarkeyIntegrationConfiguration> intConf,
        ILogger<MalarkeyServerAuthenticationHandler> logger,
        IMalarkeyServerAuthenticationEventHandler events) : base(
            options: options,
            loggerFactory: loggerFactory,
            encoder: encoder,
            synchronizer: synchronizer,
            tokenHandler: tokenIssuer,
            sessionCache: sessionCache,
            flowHandlers: flowHandlers,
            intConf: intConf,
            logger: logger)
    {
        _profileRepo = profileRepo;
        _eventHandler = events;
    }

    protected override async Task<MalarkeyProfileAndIdentities> ConstructProfile(MalarkeyAuthenticationSession session, MalarkeyProfileIdentity identity)
    {
        if (session.ExistingProfileId != null)
        {
            var profileAndIdentities = await _profileRepo.AttachIdentityToProfile(identity, session.ExistingProfileId.Value);
            return profileAndIdentities;
        }
        else
        {
            var profileAndIdentities = await _profileRepo.LoadOrCreateByIdentity(identity);
            return profileAndIdentities!;
        }
    }

    protected override void PublisizeRegistrationEvent(MalarkeyProfileIdentity ident)
    {
        _eventHandler.RegisterIdentificationCompleted(ident);
    }







}
