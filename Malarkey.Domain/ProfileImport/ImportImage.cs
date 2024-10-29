using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.ProfileImport;
public record ImportImage(
    byte[] Data,
    string FileType
    )
{
    public string AsBase64Data = $"{Convert.ToBase64String(Data)}";
    public string AsImageDataAttribute => $"data:{FileType};base64, {AsBase64Data}";
} 