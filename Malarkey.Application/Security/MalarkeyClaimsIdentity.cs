using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
public class MalarkeyClaimsIdentity : ClaimsIdentity
{
    public MalarkeyClaimsIdentity(IEnumerable<Claim> claims) : base(
        claims: claims,
        authenticationType: nameof(Malarkey).ToLower(),
        nameType: MalarkeyClaimsPrincipal.MalarkeyNameClaimType,
        roleType: null
        )
    {
    }


}
