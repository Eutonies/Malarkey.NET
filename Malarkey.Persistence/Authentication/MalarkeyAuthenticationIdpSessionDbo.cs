using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Malarkey.Abstractions.Util;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationIdpSessionDbo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long IdpSessionId { get; set; }
    public long SessionId { get; set; }
    public MalarkeyIdentityProviderDbo IdProvider { get; set; }
    public string? Nonce { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeVerifier { get; set; }
    public DateTime InitTime { get; set; }
    public DateTime? AuthenticatedTime { get; set; }
    public string Scopes { get; set; }




    


    public MalarkeyAuthenticationIdpSession ToDomain() => new MalarkeyAuthenticationIdpSession(
        IdpSessionId : IdpSessionId,
        SessionId: SessionId,
        IdProvider: IdProvider.ToDomain(),
        Nonce: Nonce,
        CodeChallenge: CodeChallenge,
        CodeVerifier: CodeVerifier,
        InitTime: InitTime,
        AuthenticatedTime: AuthenticatedTime,
        Scopes: Scopes.Split(" ")
    );



}


internal static class MalarkeyAuthenticationIdpSessionDboExtensions
{
    public static MalarkeyAuthenticationIdpSessionDbo ToDbo(this MalarkeyAuthenticationIdpSession sess) => new MalarkeyAuthenticationIdpSessionDbo
    {
        IdpSessionId = sess.IdpSessionId,
        SessionId = sess.SessionId,
        IdProvider = sess.IdProvider.ToDbo(),
        Nonce = sess.Nonce,
        CodeChallenge = sess.CodeChallenge,
        CodeVerifier = sess.CodeVerifier,
        InitTime = sess.InitTime,
        AuthenticatedTime = sess.AuthenticatedTime,
        Scopes = sess.Scopes.MakeString(" ")
    };


}
