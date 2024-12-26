using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public sealed record MalarkeyProfileToken(
    Guid TokenId,
    string IssuedTo,
    DateTime IssuedAt,
    DateTime ValidUntil,
    MalarkeyProfile Profile
    ) : MalarkeyToken(TokenId, IssuedTo, IssuedAt, ValidUntil);
