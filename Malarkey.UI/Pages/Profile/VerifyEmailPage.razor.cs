using Malarkey.Application.Profile;
using Microsoft.AspNetCore.Components;

namespace Malarkey.UI.Pages.Profile;

public partial class VerifyEmailPage
{
    [SupplyParameterFromQuery(Name = IVerificationEmailSender.EmailIdQueryParameterName)]
    [Parameter]
    public string EmailId { get; set; }

    [SupplyParameterFromQuery(Name = IVerificationEmailSender.CodeStringQueryParameterName)]
    [Parameter]
    public string CodeString { get; set; }

    [Inject]
    public IVerificationEmailHandler EmailHandler { get; set; }

    private string? _errorMessage;
    private string? _successMessage;

    protected override async Task OnParametersSetAsync()
    {
        if (_errorMessage == null && _successMessage == null)
        {
            try
            {
                if (!long.TryParse(EmailId, out var id))
                    _errorMessage = $"Email ID: {EmailId} is not valid";
                else
                {
                    var regRes = await EmailHandler.RegisterVerification(id, CodeString);
                    if (regRes != null)
                        _successMessage = $"Succesfully verified email address: {regRes.EmailAddress}";
                    else
                        _errorMessage = "I don't know what but SOMETHING went haywire!";
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            _ = InvokeAsync(StateHasChanged);
        }


    }


}
