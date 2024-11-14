using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;

namespace Malarkey.UI.Session;

public record MalarkeyUser(
    MalarkeyProfile Profile,
    IReadOnlyCollection<ProfileIdentity> Identities
    );
