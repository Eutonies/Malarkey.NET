﻿using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationSession(
    long SessionId,
    string State,
    bool IsInternal,
    DateTime InitTime,
    string SendTo,
    string? RequestedSendTo,
    MalarkeyIdentityProvider? RequestedIdProvider,
    string? RequestState,
    string[]? RequestedScopes,
    DateTime? AuthenticatedTime,
    Guid? ProfileTokenId,
    Guid? IdentityTokenId,
    string Audience,
    Guid? ExistingProfileId,
    bool AlwaysChallenge,
    IReadOnlyCollection<MalarkeyAuthenticationSessionParameter> RequestParameters,
    MalarkeyAuthenticationIdpSession? IdpSession,
    bool EncryptState
    )
{
    public MalarkeyAuthenticationParameters ToParameters() => new MalarkeyAuthenticationParameters(
        IsInternal: IsInternal,
        RequestedSendTo: RequestedSendTo,
        RequestedIdProvider: RequestedIdProvider,
        RequestState: RequestState,
        RequestedScopes: RequestedScopes,
        ExistingProfileId: ExistingProfileId,
        AlwaysChallenge: AlwaysChallenge
        );


}

