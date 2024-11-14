using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Security;
public class MalarkeyClaimsPrincipal : ClaimsPrincipal
{
    public const string MalarkeyIdClaimType = "malid";
    public const string MalarkeyNameClaimType = "malnam";
    public const string MalarkeyIdentityName = "malarkey";
    public const string IdentityTypeName = "idtyp";
    public MalarkeyProfileToken ProfileToken { get; private set; }
    public MalarkeyClaimsPrincipal(MalarkeyProfileToken profileToken, IEnumerable<MalarkeyIdentityToken> externalIdentities) 
    {
        ProfileToken = profileToken;

        AddIdentity(new MalarkeyClaimsIdentity(claims: [
            new Claim(IdentityTypeName, MalarkeyIdentityName),
            new Claim(MalarkeyIdClaimType, profileToken.Profile.ProfileId.ToString()),
            new Claim(MalarkeyNameClaimType, profileToken.Profile.ProfileName)
           ])
        );
        foreach(var extId in externalIdentities)
        {
            AddIdentity(new MalarkeyClaimsIdentity(claims: [
                new Claim(IdentityTypeName, extId.Identity.IdentityType),
                        new Claim(MalarkeyIdClaimType, extId.Identity.ProviderId),
                        new Claim(MalarkeyNameClaimType, extId.Identity.FirstName)
               ])
            );
        }
    }

}
