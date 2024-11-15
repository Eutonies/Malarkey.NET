using Malarkey.Application.Common;
using Malarkey.Application.Security;
using Malarkey.Domain.Profile;
using Malarkey.Domain.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile;
internal class ProfileService : IProfileService
{

    private readonly IMalarkeyTokenHandler _tokenHandler;

    public ProfileService(IMalarkeyTokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler;
    }

    public async Task<ActionResult<MalarkeyProfile>> ExtractProfileFromToken(string token, string receiverCertificate) =>
        (await _tokenHandler.ValidateToken(token, receiverCertificate)) switch
        {
            MalarkeyTokenValidationSuccessResult succ => new SuccessActionResult<MalarkeyProfile>((succ.ValidToken as MalarkeyProfileToken)!.Profile),
            MalarkeyTokenValidationExceptionResult exc => new ExceptionActionResult<MalarkeyProfile>(exc.Exception),
            _ => new UnauthorizedActionResult<MalarkeyProfile>("Not happening buddy")
        };

    public async Task<ActionResult<string>> IssueSampleProfileToken(string receiverCertificate)
    {
        var profile = new MalarkeyProfile(Guid.NewGuid(), "Sample profile", DateTime.Now, null);
        var (token, tokenString) = await _tokenHandler.IssueToken(profile, receiverCertificate);
        return new SuccessActionResult<string>(tokenString);
    }

    public Task<ActionResult<MalarkeyProfileAndIdentities>> LoadOrCreateProfile(ProfileIdentity identity)
    {
        throw new NotImplementedException();
    }
}
