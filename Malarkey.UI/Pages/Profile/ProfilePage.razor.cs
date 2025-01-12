using Malarkey.Abstractions.Profile;
using Malarkey.Application.Common;
using Malarkey.Application.Profile.Persistence;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;
using System.Net.Mail;
namespace Malarkey.UI.Pages.Profile;
public partial class ProfilePage 
{
    [Inject]
    public NavigationManager NavManager { get; set; }

    [Inject]
    public IMalarkeyProfileRepository ProfileRepo { get; set; }

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }
    private Guid? ProfileId => SessionState.User?.Profile?.ProfileId;

    private DateTime _lastUpdate = DateTime.Now;
    private string? _infoMessage;

    private MalarkeyProfile? _profile;
    private IReadOnlyCollection<MalarkeyProfileIdentity> _identities = [];
    public string? ProfileName { 
        get => _profile?.ProfileName; 
        set {
            var newValue = value;
            if(ProfileId != null && newValue != null)
            {
                Task.Run(async () =>
                {
                    var saveResult = await ProfileRepo.UpdateProfileName(ProfileId.Value, newValue);
                    if (saveResult is SuccessActionResult<MalarkeyProfile> succ)
                    {
                        _profile = succ.Result;
                        _profileNameError = null;
                    }
                    else if(saveResult is ErrorActionResult<MalarkeyProfile> err)
                    {
                        _profileNameError = err.ErrorMessage;
                    }
                    await InvokeAsync(StateHasChanged);
                });

            }
        }
    }

    private string? _profileNameError;

    public string? FirstName
    {
        get => _profile?.FirstName;
        set => UpdateAndReload((repo, profId) => repo.UpdateFirstName(profId, value), "first name");
     }
    public string? LastName
    {
        get => _profile?.LastName;
        set => UpdateAndReload((repo, profId) => repo.UpdateLastName(profId, value), "last name");
    }

    public string? PrimaryEmail
    {
        get => _profile?.PrimaryEmail;
        set
        {
            if(!string.IsNullOrWhiteSpace(value) && !IsValidEmail(value))
            {
                _primaryEmailError = "Invalid email";
            }
            else
            {
                UpdateAndReload((repo, profId) => repo.UpdatePrimaryEmail(profId, value), "email");
                _primaryEmailError = null;
            }

        }
    }
    private string? _primaryEmailError;
    private bool IsValidEmail(string email) => MailAddress.TryCreate(email, out _);

    protected override async Task OnInitializedAsync()
    {
        if(_profile == null && SessionState.User != null)
        {
            var loaded = await ProfileRepo.LoadProfileAndIdentities(SessionState.User!.Profile.ProfileId);
            if(loaded != null)
            {
                _profile = loaded.Profile;
                _identities = loaded.Identities;
            }
            await InvokeAsync(StateHasChanged);

        }
    }

    private void UpdateAndReload(Func<IMalarkeyProfileRepository, Guid, Task<ActionResult<MalarkeyProfile>>> repoAction, string fieldName) => Task.Run(async () =>
    {
        if(ProfileId != null)
        {
            var result = await repoAction(ProfileRepo, ProfileId.Value);
            if (result is SuccessActionResult<MalarkeyProfile> succ)
            {
                _profile = succ.Result;
                await InvokeAsync(StateHasChanged);
                FlashInfo($"Updated {fieldName}");
            }

        }
    });

    private void FlashInfo(string infoText)
    {
        _infoMessage = infoText;
        _lastUpdate = DateTime.Now;
        InvokeAsync(StateHasChanged);
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _infoMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }


}