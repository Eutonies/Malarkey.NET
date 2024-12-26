
using Malarkey.Abstractions.Token;

namespace Malarkey.Abstractions.Profile;
public sealed record SpotifyIdentity(
    Guid IdentityId,
    Guid ProfileId,
    string SpotifyId,
    string Name,
    string? MiddleNames,
    string? LastName,
    string? Email,
    IdentityProviderToken? AccessToken
    ) : MalarkeyProfileIdentity(
        IdentityId,
        ProfileId,
        SpotifyId,
        Name,
        MiddleNames,
        LastName
        )
{
    public override MalarkeyIdentityProvider IdentityProvider => MalarkeyIdentityProvider.Spotify;

}
