using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class HttpExtensions
{

    public static async Task<string> ReadAsStringAsync(this Stream requestBody, bool leaveOpen = false)
    {
        using var reader = new StreamReader(requestBody, leaveOpen: leaveOpen);
        var bodyAsString = await reader.ReadToEndAsync();
        return bodyAsString;
    }

    public static FormUrlEncodedContent ToFormContent(this IEnumerable<(string, string)> formData) => new FormUrlEncodedContent(
            formData
            .Where(_ => _.Item2 != null)
            .Select(_ => new KeyValuePair<string, string>(_.Item1, _.Item2))
        );





}
