using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationSessionParameter(
    long SessionId,
    string Name,
    string Value
    )
{
    public string NameKey => Name.ToLower().Trim();

}
