using Microsoft.AspNetCore.Authentication;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class DataExtensions
{
    public static string ToBase64String(this byte[] data) =>
        Convert.ToBase64String(data);

    public static async Task<byte[]> ToByteArray(this Stream stream)
    {
        using var tempStream = new MemoryStream();
        await stream.CopyToAsync(tempStream);
        return tempStream.ToArray();
    }

}
