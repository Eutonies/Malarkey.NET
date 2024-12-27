using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Domain.Util;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.Json;

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
                ProfileName: Payload.name,
                CreatedAt: DateTime.UnixEpoch + TimeSpan.FromSeconds(Payload.crets!.Value),
                AbsorbedBy: Payload.absby == null ? null : Guid.Parse(Payload.absby)
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
                    IdentityId: Guid.Parse(Payload.id),
                    ProfileId: Guid.Parse(Payload.sub),
                    MicrosoftId: Payload.identid!,
                    PreferredName: Payload.prefname!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname
                    ),
                MalarkeyTokenIdentityTypeTso.Google => new GoogleIdentity(
                    IdentityId: Guid.Parse(Payload.id),
                    ProfileId: Guid.Parse(Payload.sub),
                    GoogleId: Payload.identid!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname
                    ),
                MalarkeyTokenIdentityTypeTso.Spotify => new SpotifyIdentity(
                    IdentityId: Guid.Parse(Payload.id),
                    ProfileId: Guid.Parse(Payload.sub),
                    SpotifyId: Payload.identid!,
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname,
                    Email: Payload.email,
                    AccessToken: Payload.idptoken?.Pipe(idpt => new IdentityProviderToken(
                        Token: idpt.token,
                        Issued: idpt.iat.ParseJwtTime(),
                        Expires: idpt.exp.ParseJwtTime(),
                        RefreshToken: idpt.refresh,
                        Scopes: idpt.scopes.Split(" ")
                        )
                    )
                    ),

                _ => new FacebookIdentity(
                    IdentityId: Guid.Parse(Payload.id),
                    ProfileId: Guid.Parse(Payload.sub),
                    FacebookId: Payload.identid!,
                    PreferredName: Payload.prefname ?? "Unknown",
                    Name: Payload.name,
                    MiddleNames: Payload.midnames,
                    LastName: Payload.lastname,
                    Email: Payload.email
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
    string id,
    string? identtyp = null,
    string? identid = null,
    string? prefname = null,
    string? midnames = null,
    string? lastname = null,
    string iss = MalarkeySecurityConstants.TokenIssuer,
    long? crets = null,
    string? absby = null,
    string? email = null,
    MalarkeyIdProviderAccessTokenTso? idptoken = null
    );

internal record MalarkeyIdProviderAccessTokenTso(
    long iat,
    long exp,
    string token,
    string? refresh,
    string scopes
    );


internal enum MalarkeyTokenTypeTso
{
    Identity = 1,
    Profile = 2
}

internal enum MalarkeyTokenIdentityTypeTso
{
    Microsoft = 10,
    Google = 20,
    Facebook = 30,
    Spotify = 40
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
                jti: tokenId.ToString(),
                id: profile.ProfileId.ToString(),
                crets: (long) (profile.CreatedAt - DateTime.UnixEpoch).TotalSeconds,
                absby: profile.AbsorbedBy?.ToString()
                );

    internal static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyProfileIdentity ident,
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
                id: ident.IdentityId.ToString(),
                identtyp: ident.IdentityProviderType(),
                identid: ident.ProviderId,
                prefname: (ident as MicrosoftIdentity)?.PreferredName,
                midnames: ident.MiddleNames,
                lastname: ident.LastName
               );

    internal static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyProfileToken token,
        string receiver
        ) => new MalarkeyTokenPayloadTso(
                iat: token.IssuedAt.ToJwtTime(),
                sub: token.Profile.ProfileId.ToString(),
                aud: receiver,
                name: token.Profile.ProfileName,
                exp: token.ValidUntil.ToJwtTime(),
                jti: token.TokenId.ToString(),
                id: token.Profile.ProfileId.ToString(),
                crets: (long)(token.Profile.CreatedAt - DateTime.UnixEpoch).TotalSeconds,
                absby: token.Profile.AbsorbedBy?.ToString()
               );

    internal static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyIdentityToken token,
        string receiver
        ) => new MalarkeyTokenPayloadTso(
                iat: MalarkeySecurityConstants.Now.ToJwtTime(),
                sub: token.Identity.ProfileId.ToString(),
                aud: receiver,
                name: token.Identity.FirstName,
                exp: token.ValidUntil.ToJwtTime(),
                jti: token.TokenId.ToString(),
                id: token.Identity.IdentityId.ToString(),
                identtyp: token.Identity.IdentityProviderType(),
                identid: token.Identity.ProviderId,
                prefname: (token.Identity as MicrosoftIdentity)?.PreferredName,
                midnames: token.Identity.MiddleNames,
                lastname: token.Identity.LastName,
                email: token.Identity.EmailToUse,
                idptoken: token.Identity.IdentityProviderTokenToUse?.Pipe(_ => _.ToPayloadTso())
               );


    internal static MalarkeyIdProviderAccessTokenTso ToPayloadTso(this IdentityProviderToken token) => new MalarkeyIdProviderAccessTokenTso(
        iat: token.Issued.ToJwtTime(),
        exp: token.Expires.ToJwtTime(),
        token: token.Token,
        refresh: token.RefreshToken,
        scopes: token.Scopes.MakeString(" ")
    );



    internal static string IdentityProviderType(this MalarkeyProfileIdentity ident) => ident switch
    {
        MicrosoftIdentity _ => MalarkeyTokenIdentityTypeTso.Microsoft.ToString(),
        GoogleIdentity _ => MalarkeyTokenIdentityTypeTso.Google.ToString(),
        SpotifyIdentity _ => MalarkeyTokenIdentityTypeTso.Spotify.ToString(),
        _ => MalarkeyTokenIdentityTypeTso.Facebook.ToString()
    };
   


}