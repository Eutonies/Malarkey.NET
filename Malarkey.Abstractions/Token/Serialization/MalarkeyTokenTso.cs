using Malarkey.Abstractions;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Token;
using Malarkey.Abstractions.Util;
using System.Globalization;
using System.Text.Json;

namespace Malarkey.Security.Formats;
public record MalarkeyTokenTso(
    MalarkeyTokenHeaderTso Header,
    MalarkeyTokenPayloadTso Payload,
    string Signature
    )
{


}

public record MalarkeyTokenHeaderTso(
    string toktyp,
    string alg = MalarkeyConstants.Authentication.TokenAlgorithm,
    string typ = MalarkeyConstants.Authentication.TokenType
    );

public record MalarkeyTokenPayloadTso(
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
    string iss = MalarkeyConstants.Authentication.TokenIssuer,
    string? crets = null,
    string? absby = null,
    string? email = null,
    MalarkeyIdProviderAccessTokenTso? idptoken = null,
    string? firstname = null
    );

public record MalarkeyIdProviderAccessTokenTso(
    long iat,
    long exp,
    string token,
    string? refresh,
    string scopes
    );


public enum MalarkeyTokenTypeTso
{
    Identity = 1,
    Profile = 2
}

public enum MalarkeyTokenIdentityTypeTso
{
    Microsoft = 10,
    Google = 20,
    Facebook = 30,
    Spotify = 40
}
