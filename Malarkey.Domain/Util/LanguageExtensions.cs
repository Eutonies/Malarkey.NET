using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Util;
public static class LanguageExtensions
{

    public static B Pipe<A, B>(this A input, Func<A, B> func) => func(input);



}
