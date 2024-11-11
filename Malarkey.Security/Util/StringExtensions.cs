using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Malarkey.Security.Util;
internal static class StringExtensions
{

    private static readonly Regex MetaRegex = new Regex(@"^-{5}((BEGIN)|(END)).*");

    public static string CleanCertificate(this string cert) => string.Join("", cert
        .Split("\n")
        .Select(_ => _.Trim())
        .Where(_ => !MetaRegex.IsMatch(_)));
        

}
