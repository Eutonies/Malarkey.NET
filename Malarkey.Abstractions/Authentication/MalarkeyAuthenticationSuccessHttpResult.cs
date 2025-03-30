using Malarkey.Abstractions;
using Malarkey.Abstractions.Authentication;
using Malarkey.Abstractions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Malarkey.Abstractions.Authentication;
public record MalarkeyAuthenticationSuccessHttpResult(
    MalarkeyAuthenticationSession Session,
    string ProfileToken,
    IReadOnlyCollection<string> IdentityTokens,
    ILogger Logger
    ) : IResult
{
    private static readonly Encoding _utf8 = UTF8Encoding.UTF8;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await Task.CompletedTask;
        var urlHolder = new StringBuilder($"{Session.SendTo}");
        if(Session.IsInternal && Session.RequestParameters.Any())
        {
            urlHolder.Append("?" + Session.RequestParameters
                 .Select(_ => $"{_.Name}=${_.Value.UrlEncoded()}")
                 .MakeString("&")
            );
        }
        var url = urlHolder.ToString();
        if(url.StartsWith("/"))
          url = url.Substring(1);
        // Status code 303: redirect to Headers.Location with Http Method GET
        httpContext.Response.StatusCode = 303;
        DebugLog($"Malarkey Authentication Success Result - HTTP Status Code: {httpContext.Response.StatusCode}");
        DebugLog($"   Profile token of {ProfileToken.Length} chars: {MalarkeyConstants.Authentication.ProfileCookieName}={ProfileToken}");
        httpContext.Response.Cookies.Append(MalarkeyConstants.Authentication.ProfileCookieName, ProfileToken);
        DebugLog($"   Identity tokens:");
        var identList = IdentityTokens.ToList();
        for(var identIndx = 0; identIndx < identList.Count; identIndx++) {
            var cookieName = MalarkeyConstants.Authentication.IdentityCookieName(identIndx);
            var idTok = identList[identIndx];
           httpContext.Response.Cookies.Append(cookieName, idTok);
           DebugLog($"    {cookieName}={idTok}\n  ID token is of {idTok.Length} chars");
        }
        DebugLog($"URL: {url}");
        httpContext.Response.Headers.Location = url;
    }


    private void DebugLog(string str) => 
      Logger.LogInformation(str);


    private record ReturnBody(string ProfileToken, IReadOnlyCollection<string> IdentityTokens);  



    private const string SampleToken = "eyJhbGciOiJQUzI1NiIsInRva3R5cCI6IlByb2ZpbGUiLCJ0eXAiOiJKV1QifQ.eyJhdWQiOiJNSUlEMlRDQ0FzR2dBd0lCQWdJVU1makxmb0wreWdPeFo4WXBoRnhSUzd6WWo3b3dEUVlKS29aSWh2Y05BUUVMQlFBd1p6RUxNQWtHQTFVRUJoTUNSRXN4RXpBUkJnTlZCQWNNQ2tOdmNHVnVhR0ZuWlc0eEZUQVRCZ05WQkFvTURFVjFkRzl1YVdWekxtTnZiVEVWTUJNR0ExVUVDd3dNVFdGc1lYSnJaWGt1VGtWVU1SVXdFd1lEVlFRRERBeGxkWFJ2Ym1sbGN5NWpiMjB3SGhjTk1qUXhNREkzTVRnME56QXhXaGNOTWpZeE1ESTNNVGcwTnpBeFdqQm5NUXN3Q1FZRFZRUUdFd0pFU3pFVE1CRUdBMVVFQnd3S1EyOXdaVzVvWVdkbGJqRVZNQk1HQTFVRUNnd01SWFYwYjI1cFpYTXVZMjl0TVJVd0V3WURWUVFMREF4TllXeGhjbXRsZVM1T1JWUXhGVEFUQmdOVkJBTU1ER1YxZEc5dWFXVnpMbU52YlRDQ0FTSXdEUVlKS29aSWh2Y05BUUVCQlFBRGdnRVBBRENDQVFvQ2dnRUJBTXQvMVdpb094UDAzKzIvUEh5YU1vN1ZOQSt3SFpMTDI4YXRZNlovRjRHU0p5MmFmVXRPV2hXYmYvZ214czF2aEZuUU9SVVRYMkU2NXpibVNqY0NLZWVDQnhKTi9HV3liOTMyVlVRMjdaSXNod2plYlB6Y1lSL1RTRE9HQTFyR2xlSElCNHNXeXlnZVFaWFBlNXVaOEU3RlpFT0ZsdHpzWE81Q01udGR6S3czbjN0L3owTk5XMHNST1JWcDI1Zzk0amhtK0pXTUxGYmt5YmtNakJBM2xEay8wd2tlQTVzUm5ndTNRQXF3WEM0c3A4VUZaR25ZTzRHODlURWdwaDdONjNObm9uaW9sUUhyVmFRVUpld2w2dzZOY0tvL0l3SHJSeVppbnc0ejIrS2dKMlZmb3F0cVRrWVlhbkxoMUhYZWR4bFBseFNNeDNRbHNxdTZBNk5BbW84Q0F3RUFBYU45TUhzd0N3WURWUjBQQkFRREFnUXdNQk1HQTFVZEpRUU1NQW9HQ0NzR0FRVUZCd01CTURnR0ExVWRFUVF4TUMrQ0VIZDNkeTVsZFhSdmJtbGxjeTVqYjIyQ0RHVjFkRzl1YVdWekxtTnZiWUlOWlhWMGIyNXBaWE11WTI5dExqQWRCZ05WSFE0RUZnUVVjN3pGMHhmY2FXcjZta1JyQXlVQ0lMTGFCU1V3RFFZSktvWklodmNOQVFFTEJRQURnZ0VCQUlESFMwTENHaEY4ZmYwR3JWajlkUDRwUnVXTmNiblpWeWZWR2VBWS96blZLU0lNUk81S3djdFBnMDlMQVprVEwwbzFFSHA0TWNzNGczYWlrME1MWVJBYjJhQ2hwVFNpaDdNZnQ5L1o1VTJuSFIrTmFmbGpQSUJadU1GZU5ISUlGR1A4VEVQeExwV0g5dURFYnlkN1FwbmZELzNvNGhoUmYrd1U5WHdGQ1pVeUs2Q3dneHdKa0VaZENRM1ZkdzhUSngwTkNPMXNIK3FhVnhvdVZiTE9qRWYvNTZvblQwbm5xdWdNdzlnZ09qQXFPWjdpYnBYTmMvS0xVZXFNOGpraWxoR0c0VlQyMjY2WXZySWpueE52QzVxQ0syYXF4ZzBNTWFlZlp6YnRCWU5pV3VPNVhRUmV5cFhNR042VDdkcGhVdWJHZWtCNTRiS2oyODJJM3M2bk9xZz0iLCJpc3MiOiJldXRvbmllcy5jb20vbWFsYXJrZXkiLCJleHAiOjE3MzkwMjQ1MjcsImlhdCI6MTczOTAyMDkyNywibmJmIjoxNzM5MDIwOTI3LCJzdWIiOiIwMTk0YzQ2MC1mYzY2LTc2YzgtYWQ4Zi01MGZiMGMyMTIwZTkiLCJpZCI6IjAxOTRjNDYwLWZjNjYtNzZjOC1hZDhmLTUwZmIwYzIxMjBlOSIsImp0aSI6ImRiZWEwN2M0LTVkZTAtNGI4ZC04OTlhLTY0YTYyNjAxYWRiOSIsIm5hbWUiOiJzdW5lX3JvZW5uZUBvdXRsb29rLmNvbSIsImNyZXRzIjoiMTczODQ2MTQ3NyJ9.hvII2-sJnjPgfgjR4uUhcqmDYBLGBY5r5IJ26t4ev2RCvSe3hAbtsjrjzXM7o1XWp6ILhHDgq8j720_3xusoBwzw7Fmo1g4fZvo1Hh-WAZq2WZBl5oxK9TeKTASM2bIsaGUSKOFAWZELIx7Ni46sAvgA4oKLSQWBfyZb7AQDdHHPNIy89o4rcHXMEP3bAzuiN2q9nbjaBf74Ill8goD7qpolFfcqnskHf9PJkH795U77w4y9nCGV_FbbCCpvmqOQFfvXXWHN6XbefrGNC1UWLvTILNQdLrdeS6RfUy2pMVNNldZeQCkNAkLs_iLu3uACBiL28OtrIr5JgKcZmkm7FA";



}
