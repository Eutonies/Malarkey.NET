using Malarkey.Application.Common;
using Malarkey.Domain.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile;
public interface IProfileService
{
    Task<ActionResult<string>> IssueSampleProfileToken(string receiverCertificate);
    Task<ActionResult<MalarkeyProfile>> ExtractProfileFromToken(string token, string receiverCertificate);
    Task<ActionResult<MalarkeyProfileAndIdentities>> LoadOrCreateProfile(ProfileIdentity identity);
}
