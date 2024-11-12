using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Malarkey.Security.Formats;
internal record MalarkeyTokenTso(
    MalarkeyTokenHeaderTso Header,
    MalarkeyTokenPayloadTso Payload,
    string Signature
    )
{
    public override string ToString() => $"{Header.Serialize()}.{Payload.Serialize()}.{Signature.InBase64()}";

    public MalarkeyToken ToDomain() => Enum.Parse<MalarkeyTokenTypeTso>(Header.toktyp) switch
    {
        MalarkeyTokenTypeTso.Profile => new MalarkeyProfileToken(
            TokenId: Guid.Parse(Payload.jti),
            IssuedTo: Payload.aud,
            IssuedAt: Payload.iat.ParseJwtTime(),
            ValidUntil: Payload.exp.ParseJwtTime(),
            Profile: new MalarkeyProfile(
                ProfileId: Guid.Parse(Payload.sub),
                ProfileName: Payload.name
                )
            ),
        _ => new MalarkeyIdentityToken(
            TokenId: Guid.Parse(Payload.jti),
            IssuedTo: Payload.aud,
            IssuedAt: Payload.iat.ParseJwtTime(),
            ValidUntil: Payload.exp.ParseJwtTime(),
            Identity: Enum.Parse<MalarkeyTokenIdentityTypeTso>(Payload.identtyp!) switch
            {
                MalarkeyTokenIdentityTypeTso.Microsoft => new MicrosoftIdentity(
                    ProfileId: Guid.Parse(Payload.sub),
                    MicrosoftId: Payload.identid!,
                    PreferredName: Payload.prefname!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname
                    ),
                MalarkeyTokenIdentityTypeTso.Google => new GoogleIdentity(
                    ProfileId: Guid.Parse(Payload.sub),
                    GoogleId: Payload.identid!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname
                    ),
                _ => new FacebookIdentity(
                    ProfileId: Guid.Parse(Payload.sub),
                    FacebookId: Payload.identid!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname
                    )

            })

         };

}

internal record MalarkeyTokenHeaderTso(
    string toktyp,
    string alg = MalarkeySecurityConstants.TokenAlgorithm,
    string typ = MalarkeySecurityConstants.TokenType
    );

internal record MalarkeyTokenPayloadTso(
    long iat,
    string sub,
    string aud,
    string name,
    long exp,
    string jti,
    string? identtyp = null,
    string? identid = null,
    string? prefname = null,
    string? midnames = null,
    string? lastname = null,
    string iss = MalarkeySecurityConstants.TokenIssuer
    )
{



}

internal enum MalarkeyTokenTypeTso
{
    Identity = 1,
    Profile = 2
}

internal enum MalarkeyTokenIdentityTypeTso
{
    Microsoft = 10,
    Google = 20,
    Facebook = 30
}

internal static class MalarkeyTokenTsoExtensions
{
    private static readonly JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
    {
        WriteIndented = false
    };

    internal static string Serialize(this object obj) => JsonSerializer.Serialize(obj, _serializationOptions).InBase64();

    internal static string InBase64(this string str) => Base64UrlEncoder.Encode(str);

    internal static TPart DeserializeToTokenPart<TPart>(this string str) =>
        JsonSerializer.Deserialize<TPart>(str.FromBase64())!;

    internal static string FromBase64(this string str) => Base64UrlEncoder.Decode(str);


    internal static MalarkeyTokenTso DeserializeToMalarkeyToken(this string str)
    {
        var parts = str.Split('.');
        var utf8parts = parts
           .Select(_ => _.FromBase64())
           .ToArray();
        var header = parts[0].DeserializeToTokenPart<MalarkeyTokenHeaderTso>();
        var payload = parts[1].DeserializeToTokenPart<MalarkeyTokenPayloadTso>();
        var signatur = parts[2].FromBase64();

        var returnee = new MalarkeyTokenTso(header, payload, signatur);
        return returnee;

    }




    internal static DateTime ParseJwtTime(this long lo) => DateTime.UnixEpoch.AddSeconds(lo).ToUniversalTime();

    internal static long ToJwtTime(this DateTime tim) => (long) (tim - DateTime.UnixEpoch).TotalSeconds;


    internal static MalarkeyTokenHeaderTso ToHeaderTso(this MalarkeyToken token) => token switch
    {
        MalarkeyProfileToken _ => new MalarkeyTokenHeaderTso(toktyp: MalarkeyTokenTypeTso.Profile.ToString()),
        _ => new MalarkeyTokenHeaderTso(toktyp: MalarkeyTokenTypeTso.Identity.ToString())
    };

    internal static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyProfile profile, 
        string receiver, 
        DateTime expiresAt,
        Guid tokenId) => new MalarkeyTokenPayloadTso(
                iat: MalarkeySecurityConstants.Now.ToJwtTime(),
                sub: profile.ProfileId.ToString(),
                aud: receiver,
                name: profile.ProfileName,
                exp: expiresAt.ToJwtTime(),
                jti: tokenId.ToString()
                );

    internal static MalarkeyTokenPayloadTso ToPayloadTso(
        this ProfileIdentity ident,
        string receiver,
        DateTime expiresAt,
        Guid tokenId
        ) =>  new MalarkeyTokenPayloadTso(
                iat: MalarkeySecurityConstants.Now.ToJwtTime(),
                sub: ident.ProfileId.ToString(),
                aud: receiver,
                name: ident.FirstName,
                exp: expiresAt.ToJwtTime(),
                jti: tokenId.ToString(),
                identtyp: ident.IdentityProviderType(),
                identid: ident.ProviderId,
                prefname: (ident as MicrosoftIdentity)?.PreferredName,
                midnames: ident.MiddleNames,
                lastname: ident.LastName
               );



    internal static MalarkeyTokenTso ToTso(this MalarkeyToken token, string signature, string receiver, DateTime expiresAt) => new MalarkeyTokenTso(
        Header: token.ToHeaderTso(),
        Payload: token switch
        {
            MalarkeyProfileToken prof => prof.Profile.ToPayloadTso(receiver, expiresAt, token.TokenId),
            MalarkeyIdentityToken ident => ident.Identity.ToPayloadTso(receiver, expiresAt, token.TokenId),
            _ => throw new InvalidOperationException()
        },
        Signature: signature
    );


    internal static string IdentityProviderType(this ProfileIdentity ident) => ident switch
    {
        MicrosoftIdentity _ => MalarkeyTokenIdentityTypeTso.Microsoft.ToString(),
        GoogleIdentity _ => MalarkeyTokenIdentityTypeTso.Google.ToString(),
        _ => MalarkeyTokenIdentityTypeTso.Facebook.ToString()
    };
   


}