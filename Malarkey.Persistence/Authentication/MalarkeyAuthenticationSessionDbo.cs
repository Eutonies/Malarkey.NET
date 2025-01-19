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
    public MalarkeyIdentityProviderDbo IdProvider { get; set; }
    public string? Nonce { get; set; }
    public string? Forwarder { get; set; }
    public string CodeVerifier { get; set; }
    public string CodeChallenge { get; set; }
    public DateTime InitTime { get; set; }
    public DateTime? AuthenticatedTime { get; set; }
    public Guid? ProfileTokenId { get; set; }
    public Guid? IdentityTokenId { get; set; }
    public string? Scopes { get; set; }
    public string Audience { get; set; }
    public string? ForwarderState { get; set; }

    public Guid? ExistingProfileId { get; set; }
    


    public MalarkeyAuthenticationSession ToDomain() => new MalarkeyAuthenticationSession(
        SessionId: SessionId,
        State: State.ToString(),
        IdProvider: IdProvider.ToDomain(),
        Nonce: Nonce,
        Forwarder: Forwarder,
        CodeChallenge: CodeChallenge,
        CodeVerifier: CodeVerifier,
        InitTime: InitTime,
        AuthenticatedTime: AuthenticatedTime,
        ProfileTokenId: ProfileTokenId,
        IdentityTokenId: IdentityTokenId,
        Audience: Audience,
        Scopes: Scopes?.Split(" "),
        ForwarderState: ForwarderState,
        ExistingProfileId: ExistingProfileId
    );



}
