using Malarkey.Abstractions.Profile;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyRefreshTokenData(
    string RefreshToken,
    Guid IdentityId,
    MalarkeyIdentityProvider IdentityProvider
    );
