using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Malarkey.Abstractions.Common;
public abstract class MalarkeyHttpResult : IResult
{
    protected virtual int StatusCode => 200;

    protected abstract string Title { get; }
    protected virtual IReadOnlyCollection<(string Name, string Value)> Headers => [
        ("title", Title)
        ];

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;
        var htmlContent = GenerateHtmlPage();
        httpContext.Response.ContentType = "text/html;charset=utf-8";
        var bytes = Encoding.UTF8.GetBytes(htmlContent);
        await httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }

    protected virtual string GenerateHtmlPage()
    {
        var ret = new StringBuilder("<!DOCTYPE html>");
        ret.AppendLine("<html>");
        ret.AppendLine(" <head>");
        foreach(var head in Headers)
           ret.AppendLine($"  <{head.Name}>{head.Value}</{head.Name}>");
        ret.AppendLine(" </head>");
        ret.AppendLine(" <body>");
        var body = ProduceBody()
            .Split("\n")
            .Select(_ => $"  {_}")
            .MakeString("\n");
        ret.AppendLine(body);
        ret.AppendLine(" </body>");
        ret.AppendLine("</html>");
        return ret.ToString();
    }

    protected abstract string ProduceBody();

}
