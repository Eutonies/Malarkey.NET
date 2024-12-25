using Malarkey.Application.Profile.Persistence;
using Malarkey.Domain.Profile;
using Malarkey.Persistence.Token.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Profile.Model;
internal class MalarkeyIdentityDbo
{
    [Key]
    public Guid IdentityId { get; set; }
    public Guid ProfileId { get; set; }
    public MalarkeyIdentityProviderDbo Provider { get; set; }
    public string ProviderId { get; set; }
    public string IdentityName { get; set; }
    public string? PreferredName { get; set; }
    public string? MiddleNames { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }


    public MalarkeyProfileIdentity ToDomain(IdentityProviderTokenDbo? idProviderToken) => Provider switch
    {
        MalarkeyIdentityProviderDbo.Microsoft => new MicrosoftIdentity(IdentityId, ProfileId, ProviderId, PreferredName!, IdentityName, MiddleNames, LastName),
        MalarkeyIdentityProviderDbo.Google => new GoogleIdentity(IdentityId, ProfileId, ProviderId, IdentityName, MiddleNames, LastName),
        MalarkeyIdentityProviderDbo.Facebook => new FacebookIdentity(IdentityId, ProfileId, ProviderId, IdentityName, MiddleNames, LastName),
        MalarkeyIdentityProviderDbo.Spotify => new SpotifyIdentity(IdentityId, ProfileId, ProviderId, IdentityName, MiddleNames, LastName, Email, idProviderToken?.ToDomain()),
        _ => throw new NotImplementedException()
    };


}
