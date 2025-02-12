using System.Text;

namespace Malarkey.Abstractions.Common;
public class MalarkeyHttpForwardPostResult : MalarkeyHttpResult
{
    protected override string Title => "Malarkey Backward";
    private const string FormId = "malarkey-backward-form";

    private readonly string _url;
    private readonly IReadOnlyCollection<(string Name, string Value)> _postValues;

    protected virtual IReadOnlyCollection<(string Name, string Value)> PostValues => _postValues;

    public MalarkeyHttpForwardPostResult(string url, IEnumerable<(string Name, string Value)> postValues)
    {
        _url = url;
        _postValues = postValues.ToList();
    }

    protected override string ProduceBody()
    {
        var ret = new StringBuilder();
        ret.AppendLine($@"<form id=""{FormId}"" action=""{_url}"" method=""post"">");
        foreach (var p in PostValues)
            ret.AppendLine($@" <input type=""hidden"" name=""{p.Name}"" value=""{p.Value}"">");
        ret.AppendLine("</form>");
        ret.AppendLine("<script>");
        ret.AppendLine("  (function() {");
        ret.AppendLine($"  document.getElementById('{FormId}').submit()");
        ret.AppendLine("  })();");
        ret.AppendLine("</script>");
        return ret.ToString();
    }
}
