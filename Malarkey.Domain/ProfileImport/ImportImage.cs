using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.ProfileImport;
public record ImportImage(
    byte[] Data,
    string FileType
    ); 