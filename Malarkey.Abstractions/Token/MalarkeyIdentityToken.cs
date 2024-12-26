using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public sealed record MalarkeyIdentityToken(
    Guid TokenId,
    string IssuedTo,
    DateTime IssuedAt,
    DateTime ValidUntil,
    MalarkeyProfileIdentity Identity
    ) : MalarkeyToken(TokenId, IssuedTo, IssuedAt, ValidUntil);
