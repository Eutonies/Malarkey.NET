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
    public Task<MalarkeyProfileToken> IssueToken(MalarkeyProfile profile, string receiverPublicKey);
    public Task RecallToken(MalarkeyToken token);

    public Task<IReadOnlyCollection<TokenValidationResult>> ValidateTokens(IEnumerable<MalarkeyToken> tokens);

    public async Task<TokenValidationResult> ValidateToken(MalarkeyToken token) => (await ValidateTokens([token])).First();


}
