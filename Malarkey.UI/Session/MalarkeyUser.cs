using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;

namespace Malarkey.UI.Session;

public record MalarkeyUser(
    MalarkeyProfile Profile,
    IReadOnlyCollection<MalarkeyProfileIdentity> Identities
    );
