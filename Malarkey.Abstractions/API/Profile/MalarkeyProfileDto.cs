using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.API.Profile;
public record MalarkeyProfileDto(
    Guid ProfileId,
    string ProfileName,
    DateTime CreatedAt
    );
