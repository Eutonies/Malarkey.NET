using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class StringExtensions
{

    private static readonly Regex MetaRegex = new Regex(@"^-{5}((BEGIN)|(END)).*");

    public static string CleanCertificate(this string cert) => string.Join("", cert
        .Split("\n")
        .Select(_ => _.Trim())
        .Where(_ => !MetaRegex.IsMatch(_)));

    public static string UrlEncoded(this string input) => UrlEncoder.Default.Encode(input);

    public static string UrlDecoded(this string input) => WebUtility.UrlDecode(input);

    public static string Base64UrlEncoded(this string input)
    {
        var asBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        var returnee = asBase64
            .Replace("=", "")
            .Replace('+','-')
            .Replace('/','_');
        return returnee;
    }
    public static string Base64UrlDecoded(this string input)
    {
        var replaced = input
            .Replace('-', '+')
            .Replace('_', '/');
        var padded = (replaced.Length % 4) switch
        {
            0 => replaced,
            2 => replaced + "==",
            3 => replaced + "=",
            _ => throw new Exception("Illegal base 64 string")
        };
        var returnee = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        return returnee;
    }


}
