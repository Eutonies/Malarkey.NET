using Malarkey.Abstractions.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public sealed record MalarkeyProfileToken(
    Guid TokenId,
    string IssuedTo,
    DateTime IssuedAt,
    DateTime ValidUntil,
    MalarkeyProfile Profile
    ) : MalarkeyToken(TokenId, IssuedTo, IssuedAt, ValidUntil)
{
    public override MalarkeyProfileToken WithId(Guid tokenId) => new MalarkeyProfileToken(
        TokenId: tokenId,
        IssuedTo: IssuedTo,
        IssuedAt: IssuedAt,
        ValidUntil: ValidUntil,
        Profile: Profile
   );

}
