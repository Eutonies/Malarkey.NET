using Malarkey.Abstractions.Token.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Token;
public abstract record MalarkeyToken
{
    public Guid TokenId { get; private set; }
    public string IssuedTo { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ValidUntil { get; private set; }

    internal MalarkeyToken(
        Guid tokenId, 
        string issuedTo,
        DateTime issuedAt, 
        DateTime validUntil    
) 
    {
        TokenId = tokenId;
        IssuedTo = issuedTo;
        IssuedAt = issuedAt;
        ValidUntil = validUntil;
    }

    public static MalarkeyIdentityToken? ParseIdentityToken(string tokenString) => ParseToken(tokenString) as MalarkeyIdentityToken;
    public static MalarkeyProfileToken? ParseProfileToken(string tokenString) => ParseToken(tokenString) as MalarkeyProfileToken;

    internal static MalarkeyToken ParseToken(string tokenString)
    {
        var tso = tokenString.DeserializeToMalarkeyToken();
        var token = tso.ToDomain();
        return token;
    }


}
