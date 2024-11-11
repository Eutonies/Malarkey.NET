using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Security;
internal interface IMalarkeyTokenHandler
{
    public Task<(MalarkeyProfileToken Token, string TokenString)> IssueToken(MalarkeyProfile profile, string receiverPublicKey);
    public Task<(MalarkeyIdentityToken Token, string TokenString)> IssueToken(ProfileIdentity identity, string receiverPublicKey);

    public Task RecallToken(string tokenString);

    public Task<IReadOnlyCollection<MalarkeyTokenValidationResult>> ValidateTokens(IEnumerable<(string Token, string ReceiverPublicKey)> tokens);

    public async Task<MalarkeyTokenValidationResult> ValidateToken(string token, string receiverPublicKey) => (await ValidateTokens([(token, receiverPublicKey)])).First();


}
