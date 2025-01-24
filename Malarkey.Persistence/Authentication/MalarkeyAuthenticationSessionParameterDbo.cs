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
internal class MalarkeyAuthenticationSessionParameterDbo
{
    public long SessionId { get; set; }
    public string ParameterNameUnique { get; set; }
    public string ParameterName { get; set; }
    public string ParameterValue { get; set; }



    


    public MalarkeyAuthenticationSessionParameter ToDomain() => new MalarkeyAuthenticationSessionParameter(
        SessionId: SessionId,
        Name: ParameterName,
        Value: ParameterValue
    );

}


internal static class MalarkeyAuthenticationSessionParameterDboExtensions
{
    public static MalarkeyAuthenticationSessionParameterDbo ToDbo(this MalarkeyAuthenticationSessionParameter par) => new MalarkeyAuthenticationSessionParameterDbo
    {
        SessionId = par.SessionId,
        ParameterNameUnique = par.Name.ToLower().Trim(),
        ParameterName = par.Name.Trim(),
        ParameterValue = par.Value.Trim()
    };


}
