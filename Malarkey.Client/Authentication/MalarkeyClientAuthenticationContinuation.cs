using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Client.Authentication;
public record MalarkeyClientAuthenticationContinuation(
    string Path,
    IReadOnlyCollection<(string Name, string Value)> QueryParameters,
    string? Body
    )
{
    public static MalarkeyClientAuthenticationContinuation From(HttpRequest request) => new MalarkeyClientAuthenticationContinuation(
        Path: request.Path.Value!,
        QueryParameters: request.Query
           .Where(_ => !string.IsNullOrWhiteSpace(_.Value.ToString()))
           .Select(_ => (Name: _.Key, Value: _.Value.ToString()))
           .ToList(),
        Body: ReadBody(request)
        );

    private static string? ReadBody(HttpRequest request)
    {
        try
        {
            var body = request.Body;
            using var reader = new StreamReader(body);
            var returnee = reader.ReadToEnd();
            return returnee;
        }
        catch
        {
            return null;
        }
    }



}
