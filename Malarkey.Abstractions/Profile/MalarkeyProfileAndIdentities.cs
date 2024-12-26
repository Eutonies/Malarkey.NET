using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Profile;

public record MalarkeyProfileAndIdentities(
    MalarkeyProfile Profile,
    IReadOnlyCollection<MalarkeyProfileIdentity> Identities,
    IReadOnlyCollection<Guid>? Absorbees = null
    );
