using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Malarkey.Domain.Util;
public static class StringExtensions
{

    public static string UrlEncoded(this string input) => UrlEncoder.Default.Encode(input);


}
