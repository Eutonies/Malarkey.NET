using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Abstractions.Util;
public static class StringExtensions
{

    private static readonly Regex MetaRegex = new Regex(@"-{5}((BEGIN)|(END)).*-{5}");

    public static string CleanCertificate(this string cert) 
    {
        var newLineReplaced = cert
          .Replace("\\n", "\n");
        var newLineSplitted = newLineReplaced
          .Split("\n");
        var trimmed = newLineSplitted
            .Select(_ => _.Trim())
            .ToList();
        var cleaned = trimmed
            .Where(_ => !MetaRegex.IsMatch(_))
            .ToList();
        var returnee = cleaned.MakeString("");
        return returnee;
    }

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

    private static readonly HashAlgorithm ReceiverHasher = SHA256.Create();

    public static string HashPem(this string pem)
    {
        pem = pem
           .Replace(" ", "")
           .Replace("\r", "")
           .Replace("\n", "");
        var bytes = UTF8Encoding.UTF8.GetBytes(pem);
        var hashedBytes = ReceiverHasher.ComputeHash(bytes);
        var returnee = Convert.ToBase64String(hashedBytes);
        return returnee;
    }

}
