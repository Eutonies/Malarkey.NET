using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Profile;
public sealed record MalarkeyProfile(
    Guid ProfileId,
    string ProfileName
    );
