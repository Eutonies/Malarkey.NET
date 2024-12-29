using Malarkey.Abstractions.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;
public abstract record MalarkeyProfileIdentity
{
    internal MalarkeyProfileIdentity(
        Guid identityId,
        Guid profileId,
        string providerId,
        string firstName,
        string? middleNames,
        string? lastName
        )
    {
        IdentityId = identityId;
        ProviderId = providerId;
        ProfileId = profileId;
        FirstName = firstName;
        MiddleNames = middleNames;
        LastName = lastName;
    }
    public abstract MalarkeyIdentityProvider IdentityProvider { get; }

    public string ProviderId { get; private set; }
    public Guid IdentityId { get; private set; }

    public Guid ProfileId { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleNames { get; private set; }
    public string? LastName { get; private set; }

    public string? PreferredNameToUse => this switch
    {
        MicrosoftIdentity mic => mic.PreferredName,
        FacebookIdentity fac => fac.PreferredName,
        _ => null
    };

    public string? EmailToUse => this switch
    {
        FacebookIdentity fac => fac.Email,
        SpotifyIdentity sp => sp.Email,
        GoogleIdentity google => google.Email,
        _ => null
    };

    public IdentityProviderToken? IdentityProviderTokenToUse => this switch
    {
        SpotifyIdentity sp => sp.AccessToken,
        GoogleIdentity goog => goog.AccessToken,
        _ => null
    };

    public string ProvidersIdForIdentity => this switch
    {
        MicrosoftIdentity mi => mi.MicrosoftId,
        GoogleIdentity go => go.GoogleId,
        FacebookIdentity fa => fa.FacebookId,
        SpotifyIdentity sp => sp.SpotifyId,
        _ => throw new NotImplementedException()
    };

    public virtual MalarkeyProfileIdentity WithToken(IdentityProviderToken token) => this;


}
