using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using System.Security.Claims;

namespace Malarkey.Abstractions.Authentication;
public class MalarkeyClaimsPrincipal : ClaimsPrincipal
{
    public const string MalarkeyIdClaimType = "malid";
    public const string MalarkeyNameClaimType = "malnam";
    public const string MalarkeyIdentityName = "malarkey";
    public const string IdentityTypeName = "idtyp";
    public MalarkeyClaimsPrincipal(MalarkeyProfileToken profileToken, IEnumerable<MalarkeyIdentityToken> externalIdentities)
        : this(profileToken.Profile, externalIdentities.Select(_ => _.Identity))
    {

    }

    public MalarkeyClaimsPrincipal(MalarkeyProfile profile, IEnumerable<MalarkeyProfileIdentity> externalIdentities)
    {

        AddIdentity(new MalarkeyClaimsIdentity(claims: [
            new Claim(IdentityTypeName, MalarkeyIdentityName),
            new Claim(MalarkeyIdClaimType, profile.ProfileId.ToString()),
            new Claim(MalarkeyNameClaimType, profile.ProfileName)
           ])
        );
        foreach (var extId in externalIdentities)
        {
            AddIdentity(new MalarkeyClaimsIdentity(claims: [
                new Claim(IdentityTypeName, extId.IdentityProvider.ToString()),
                        new Claim(MalarkeyIdClaimType, extId.ProviderId),
                        new Claim(MalarkeyNameClaimType, extId.FirstName)
               ])
            );
        }
    }



}


public static class MalarkeyClaimsPrincipalExtensions
{
    public static MalarkeyClaimsPrincipal ToClaimsPrincipal(this MalarkeyProfile profile, IEnumerable<MalarkeyProfileIdentity> identities) => new MalarkeyClaimsPrincipal(
        profile,
        identities
        );

    public static MalarkeyClaimsPrincipal ToClaimsPrincipal(this MalarkeyProfileToken profile, IEnumerable<MalarkeyIdentityToken> identities) => new MalarkeyClaimsPrincipal(
        profile,
        identities
        );

}