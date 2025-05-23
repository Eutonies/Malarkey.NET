﻿using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;
public sealed record GoogleIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string GoogleId,
    string Name,
    string? MiddleNames,
    string? LastName,
    string? Email,
    IdentityProviderToken? AccessToken
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        GoogleId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override MalarkeyIdentityProvider IdentityProvider => MalarkeyIdentityProvider.Google;

    public override MalarkeyProfileIdentity WithToken(IdentityProviderToken token) => this with
    {
        AccessToken = token
    };


}

