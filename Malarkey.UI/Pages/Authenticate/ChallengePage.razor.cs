using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Profile;

namespace Malarkey.UI.Pages.Authenticate;

public partial class ChallengePage
{

    public static string BuildChallengeUrl(MalarkeyAuthenticationSession session) =>
        $"{MalarkeyConstants.Authentication.ServerChallengePath}?" +
        $"{MalarkeyConstants.AuthenticationRequestQueryParameters.SessionStateName}={session.State}";


}
