using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Security.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token.Serialization;
public static class MalarkeyTokenTsoExtensions
{
    private static readonly JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
    {
        WriteIndented = false
    };


    #region TSO => Domain

    public static MalarkeyToken ToDomain(this MalarkeyTokenTso token) => Enum.Parse<MalarkeyTokenTypeTso>(token.Header.toktyp) switch
    {
        MalarkeyTokenTypeTso.Profile => new MalarkeyProfileToken(
            TokenId: Guid.Parse(token.Payload.jti),
            IssuedTo: token.Payload.aud,
            IssuedAt: token.Payload.iat.ParseJwtTime(),
            ValidUntil: token.Payload.exp.ParseJwtTime(),
            Profile: new MalarkeyProfile(
                ProfileId: Guid.Parse(token.Payload.sub),
                ProfileName: token.Payload.name,
                CreatedAt: DateTime.UnixEpoch + TimeSpan.FromSeconds(long.Parse(token.Payload.crets!)),
                AbsorbedBy: token.Payload.absby == null ? null : Guid.Parse(token.Payload.absby),
                FirstName: token.Payload.firstname,
                LastName: token.Payload.lastname,
                PrimaryEmail: token.Payload.email,
                PrimaryEmailIsVerified: false,
                NextVerificationSendTime: null
                )
            ),
        _ => new MalarkeyIdentityToken(
            TokenId: Guid.Parse(token.Payload.jti),
            IssuedTo: token.Payload.aud,
            IssuedAt: token.Payload.iat.ParseJwtTime(),
            ValidUntil: token.Payload.exp.ParseJwtTime(),
            Identity: Enum.Parse<MalarkeyTokenIdentityTypeTso>(token.Payload.identtyp!) switch
            {
                MalarkeyTokenIdentityTypeTso.Microsoft => new MicrosoftIdentity(
                    IdentityId: Guid.Parse(token.Payload.id),
                    ProfileId: Guid.Parse(token.Payload.sub),
                    MicrosoftId: token.Payload.identid!,
                    PreferredName: token.Payload.prefname!,
                    Name: token.Payload.name,
                    MiddleNames: token.Payload.midnames,
                    LastName: token.Payload.lastname
                    ),
                MalarkeyTokenIdentityTypeTso.Google => new GoogleIdentity(
                    IdentityId: Guid.Parse(token.Payload.id),
                    ProfileId: Guid.Parse(token.Payload.sub),
                    GoogleId: token.Payload.identid!,
                    Name: token.Payload.name,
                    MiddleNames: token.Payload.midnames,
                    LastName: token.Payload.lastname,
                    Email: token.Payload.email,
                    AccessToken: token.Payload.idptoken?.Pipe(idpt => new IdentityProviderToken(
                        Token: idpt.token,
                        Issued: idpt.iat.ParseJwtTime(),
                        Expires: idpt.exp.ParseJwtTime(),
                        RefreshToken: idpt.refresh,
                        Scopes: idpt.scopes.Split(" ")
                        ))
                    ),
                MalarkeyTokenIdentityTypeTso.Spotify => new SpotifyIdentity(
                    IdentityId: Guid.Parse(token.Payload.id),
                    ProfileId: Guid.Parse(token.Payload.sub),
                    SpotifyId: token.Payload.identid!,
                    Name: token.Payload.name,
                    MiddleNames: token.Payload.midnames,
                    LastName: token.Payload.lastname,
                    Email: token.Payload.email,
                    AccessToken: token.Payload.idptoken?.Pipe(idpt => new IdentityProviderToken(
                        Token: idpt.token,
                        Issued: idpt.iat.ParseJwtTime(),
                        Expires: idpt.exp.ParseJwtTime(),
                        RefreshToken: idpt.refresh,
                        Scopes: idpt.scopes.Split(" ")
                        )
                    )
                    ),

                _ => new FacebookIdentity(
                    IdentityId: Guid.Parse(token.Payload.id),
                    ProfileId: Guid.Parse(token.Payload.sub),
                    FacebookId: token.Payload.identid!,
                    PreferredName: token.Payload.prefname ?? "Unknown",
                    Name: token.Payload.name,
                    MiddleNames: token.Payload.midnames,
                    LastName: token.Payload.lastname,
                    Email: token.Payload.email
                    )

            })

    };


    public static DateTime ParseJwtTime(this long lo) => DateTime.UnixEpoch.AddSeconds(lo).ToUniversalTime();

    #endregion

    #region Deserialization

    public static MalarkeyTokenTso DeserializeToMalarkeyToken(this string str)
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

    internal static TPart DeserializeToTokenPart<TPart>(this string str) =>
        JsonSerializer.Deserialize<TPart>(str.FromBase64())!;

    internal static string FromBase64(this string str) => str.Base64UrlDecoded();

    #endregion


    #region Domain => TSO

    public static MalarkeyTokenHeaderTso ToHeaderTso(this MalarkeyToken token) => token switch
    {
        MalarkeyProfileToken _ => new MalarkeyTokenHeaderTso(toktyp: MalarkeyTokenTypeTso.Profile.ToString()),
        _ => new MalarkeyTokenHeaderTso(toktyp: MalarkeyTokenTypeTso.Identity.ToString())
    };


    public static MalarkeyTokenPayloadTso ToPayloadTso(
    this MalarkeyProfile profile,
    string receiver,
    DateTime expiresAt,
    Guid tokenId) => new MalarkeyTokenPayloadTso(
            iat: DateTime.Now.ToJwtTime(),
            sub: profile.ProfileId.ToString(),
            aud: receiver,
            name: profile.ProfileName,
            exp: expiresAt.ToJwtTime(),
            jti: tokenId.ToString(),
            id: profile.ProfileId.ToString(),
            crets: ((long)(profile.CreatedAt - DateTime.UnixEpoch).TotalSeconds).ToString(),
            absby: profile.AbsorbedBy?.ToString(),
            lastname: profile.LastName,
            email: profile.PrimaryEmail,
            firstname: profile.FirstName
            );

    public static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyProfileIdentity ident,
        string receiver,
        DateTime expiresAt,
        Guid tokenId
        ) => new MalarkeyTokenPayloadTso(
                iat: DateTime.Now.ToJwtTime(),
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
                lastname: ident.LastName,
                firstname: ident.FirstName,
                email: ident.EmailToUse
               );

    public static MalarkeyTokenPayloadTso ToPayloadTso(
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
                crets: ((long)(token.Profile.CreatedAt - DateTime.UnixEpoch).TotalSeconds).ToString(),
                absby: token.Profile.AbsorbedBy?.ToString()
               );

    public static MalarkeyTokenPayloadTso ToPayloadTso(
        this MalarkeyIdentityToken token,
        string receiver
        ) => new MalarkeyTokenPayloadTso(
                iat: DateTime.Now.ToJwtTime(),
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


    public static MalarkeyIdProviderAccessTokenTso ToPayloadTso(this IdentityProviderToken token) => new MalarkeyIdProviderAccessTokenTso(
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

    public static long ToJwtTime(this DateTime tim) => (long)(tim - DateTime.UnixEpoch).TotalSeconds;

    #endregion

    #region Serialize

    private static string Serialize(this object obj) => JsonSerializer.Serialize(obj, _serializationOptions).InBase64();
    internal static string InBase64(this string str) => str.Base64UrlEncoded();
    public static string ToTokenString(this MalarkeyTokenTso token) => $"{token.Header.Serialize()}.{token.Payload.Serialize()}.{token.Signature.Base64UrlEncoded()}";

    public static string ToValueString(this MalarkeyIdProviderAccessTokenTso token) => token.Serialize().UrlEncoded();

    #endregion

}
