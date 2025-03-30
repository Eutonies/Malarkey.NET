using System.Text;

namespace Malarkey.Abstractions.Common;
public class MalarkeyHttpErrorMessageResult : MalarkeyHttpResult
{
    protected override string Title => "Malarkey Error";

    private readonly string _errorMessage;

    public MalarkeyHttpErrorMessageResult(string errorMessage)
    {
        _errorMessage = errorMessage;
    }

    protected override string ProduceBody()
    {
        var errorLines = _errorMessage.Split('\n');
        var ret = new StringBuilder();
        ret.AppendLine($@"<p>");
        foreach(var lin in errorLines)
            ret.AppendLine(lin);
        ret.AppendLine("</p>");
        return ret.ToString();
    }
}
