using Malarkey.Abstractions.API.Profile.Email;
using Malarkey.Application.Profile;
using Malarkey.Integration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;

namespace Malarkey.Server.Profile;
public class VerificationEmailSender : IVerificationEmailSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _emailApiBaseUrl;
    private readonly string _emailApiToken;
    private readonly string _emailSender;
    private readonly string _verifyEmailUrl;
    private readonly ILogger<VerificationEmailSender> _logger;

    private string EmailEndpointUrl => $"{_emailApiBaseUrl}email";

    public VerificationEmailSender(IHttpClientFactory httpClientFactory, IOptions<MalarkeyServerConfiguration> conf, ILogger<VerificationEmailSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _emailApiBaseUrl = conf.Value.Email.ApiBaseAddress;
        _emailApiToken = conf.Value.Email.ApiToken;
        _emailSender = conf.Value.Email.Sender;
        _verifyEmailUrl = conf.Value.Email.VerifyEmailUrl;
        _logger = logger;
    }

    public async Task SendVerificationEmail(VerifiableEmail mail)
    {
        using var client = _httpClientFactory.CreateClient();

        var verifyUrl = $"{_verifyEmailUrl}?{IVerificationEmailSender.EmailIdQueryParameterName}={mail.EmailAddressId}&{IVerificationEmailSender.CodeStringQueryParameterName}={UrlEncoder.Default.Encode(mail.CodeString)}";
        var message = new SendEmailLayout(
            from: new SendPersonLayout(email: _emailSender, "Sune-Does"),
            to: [new SendPersonLayout(
                email: mail.EmailAddress,
                name: mail.EmailAddress
                )],
            cc: [],
            bcc: [],
            subject: "Please verify onwership of email address",
            html: $"""Follow link to verify email address: <a href="{verifyUrl}">Link</a>""");

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(EmailEndpointUrl));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _emailApiToken);
        request.Content = JsonContent.Create(message);
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        _logger.LogInformation($"Sent email to verify email: {mail.EmailAddress}");
    }


    private record SendEmailLayout(
        SendPersonLayout from,
        SendPersonLayout[] to,
        SendPersonLayout[] cc,
        SendPersonLayout[] bcc,
        string? subject,
        string? html
        );

    private record SendPersonLayout(
        string email,
        string? name
        );


    private record SendResponseError(
        string? message


        );
}
