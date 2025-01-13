using Malarkey.Abstractions.API.Profile.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Profile;
public interface IVerificationEmailSender
{
    public const string EmailIdQueryParameterName = "emailid";
    public const string CodeStringQueryParameterName = "codestring";

    Task SendVerificationEmail(VerifiableEmail mail);


}
