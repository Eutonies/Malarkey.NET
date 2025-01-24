using Malarkey.Application.Profile.Persistence;
using Malarkey.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Persistence.Authentication;
internal class MalarkeyAuthenticationSessionDbo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SessionId { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid State { get; set; }
    public bool IsInternal { get; set; }
    public DateTime InitTime { get; set; }
    public string SendTo { get; set; }
    public string? RequestedSendTo { get; set; }
    public MalarkeyIdentityProviderDbo? RequestedIdentityProvider { get; set; }
    public string? RequestState { get; set; }
    public string[]? RequestedScopes { get; set; }
    public DateTime? AuthenticatedTime { get; set; }
    public Guid? ProfileTokenId { get; set; }
    public Guid? IdentityTokenId { get; set; }
    public string Audience { get; set; }
    public Guid? ExistingProfileId { get; set; }



    public MalarkeyAuthenticationSession ToDomain(IEnumerable<MalarkeyAuthenticationSessionParameterDbo> pars, MalarkeyAuthenticationIdpSessionDbo? idpSession) => new MalarkeyAuthenticationSession(
        SessionId: SessionId,
        State: State,
        IsInternal: IsInternal,
        InitTime: InitTime,
        SendTo: SendTo,
        RequestedSendTo: RequestedSendTo,
        RequestedIdProvider: RequestedIdentityProvider?.ToDomain(),
        RequestState: RequestState,
        RequestedScopes: RequestedScopes,
        AuthenticatedTime: AuthenticatedTime,
        ProfileTokenId: ProfileTokenId,
        IdentityTokenId: IdentityTokenId,
        Audience: Audience,
        ExistingProfileId: ExistingProfileId,
        RequestParameters: pars.Select(_ => _.ToDomain()).ToList(),
        IdpSession: idpSession?.ToDomain()
    );

}


internal static class MalarkeyAuthenticationSessionDboExtensions
{
    public static (
        MalarkeyAuthenticationSessionDbo Session,
        IReadOnlyCollection<MalarkeyAuthenticationSessionParameterDbo> Parameters,
        MalarkeyAuthenticationIdpSessionDbo? IdpSession
        ) ToDbo(this MalarkeyAuthenticationSession sess) => (
           Session: new MalarkeyAuthenticationSessionDbo
           {
               SessionId = sess.SessionId,
               State = sess.State,
               IsInternal = sess.IsInternal,
               InitTime = sess.InitTime,
               SendTo = sess.SendTo,
               RequestedSendTo = sess.RequestedSendTo,
               RequestedIdentityProvider = sess.RequestedIdProvider?.ToDbo(),
               RequestState = sess.RequestState,
               RequestedScopes = sess.RequestedScopes,
               AuthenticatedTime = sess.AuthenticatedTime,
               ProfileTokenId = sess.ProfileTokenId,
               IdentityTokenId = sess.IdentityTokenId,
               Audience = sess.Audience,
               ExistingProfileId = sess.ExistingProfileId
           },
           Parameters: sess.RequestParameters.Select(par => par.ToDbo()).ToList(),
           IdpSession: sess.IdpSession?.ToDbo()
           );
}
