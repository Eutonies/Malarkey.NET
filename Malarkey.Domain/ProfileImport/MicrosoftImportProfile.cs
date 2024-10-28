using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.ProfileImport;
public record MicrosoftImportProfile(
    string UserId,
    string Name,
    string? LastName,
    IReadOnlyCollection<MicrosoftImportProfile>? Contacts
    );
