using Malarkey.Abstractions.Profile;

namespace Malarkey.UI.Pages.Profile;

public record ProfileIdentityProviderEntry(
    MalarkeyIdentityProvider Provider,
    IReadOnlyCollection<MalarkeyProfileIdentity> Identities
    )
{
}
