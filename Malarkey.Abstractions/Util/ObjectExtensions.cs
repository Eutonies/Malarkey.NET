using System;
using System.Globalization;
using System.Text;

namespace Malarkey.Abstractions.Util;

public static class ObjectExtensions
{
    private static readonly CultureInfo DaDK = CultureInfo.GetCultureInfo("da-DK");
    public static string ToPropertiesString(this object obj, bool showNullValues = false) {
        if(obj == null)
           return "";
        var type = obj.GetType();
        var props = type.GetProperties();
        var returnee = new StringBuilder($"{type.Name}:");
        foreach(var prop in type.GetProperties()) 
        {
            var stringVal = prop.GetValue(obj) switch {
                null => (string?) null,
                DateTime dt when dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 => dt.ToString("dd-MM-yyyy"),
                DateTime dt => dt.ToString("dd-MM-yyyy HH:mm:ss"),
                TimeSpan ts => ts.ToString("HH:mm:ss"),
                decimal dec when Math.Abs(dec - Math.Round(dec, 3)) < 0.001m => dec.ToString("N0", DaDK),
                decimal dec => dec.ToString("N2", DaDK),
                double doub when Math.Abs(doub - Math.Round(doub, 3)) < 0.001 => doub.ToString("N0", DaDK),
                double doub => doub.ToString("N2", DaDK),
                long lon => lon.ToString("N0", DaDK),
                int i => i.ToString("N0", DaDK),
                object objVal => objVal.ToString()
            };
            if(stringVal != null || showNullValues)
              returnee.Append($"\n  {prop.Name}={stringVal}");
        }
        return returnee.ToString(); 
    }

}
