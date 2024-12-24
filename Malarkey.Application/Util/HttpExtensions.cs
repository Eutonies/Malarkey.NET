using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.Util;
public static class HttpExtensions
{

    public static async Task<string> ReadAsStringAsync(this Stream requestBody, bool leaveOpen = false)
    {
        using var reader = new StreamReader(requestBody, leaveOpen: leaveOpen);
        var bodyAsString = await reader.ReadToEndAsync();
        return bodyAsString;
    }

}
