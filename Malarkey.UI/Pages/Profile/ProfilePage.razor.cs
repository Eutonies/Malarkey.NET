using Azure.Core;
using Malarkey.Abstractions;
using Malarkey.Abstractions.API.Profile.Email;
using Malarkey.Abstractions.Profile;
using Malarkey.Abstractions.Util;
using Malarkey.Application.Common;
using Malarkey.Application.Profile;
using Malarkey.Application.Profile.Persistence;
using Malarkey.Integration.Authentication;
using Malarkey.UI.Session;
using Microsoft.AspNetCore.Components;
using System.Net.Mail;
namespace Malarkey.UI.Pages.Profile;
public partial class ProfilePage : IDisposable
{
    [Inject]
    public IMalarkeyServerAuthenticationEventHandler AuthenticationEvents { get; set; }

    [Inject]
    public NavigationManager NavManager { get; set; }

    [Inject]
    public IMalarkeyProfileRepository ProfileRepo { get; set; }

    [Inject]
    public IVerificationEmailHandler EmailHandler { get; set; }
    private bool _hasRegisteredForEmailUpdates = false;
    private bool _hasRegisteredForIdentityRegistrationUpdates = false;

    [CascadingParameter]
    public MalarkeySessionState SessionState { get; set; }
    private Guid? ProfileId => SessionState.User?.Profile?.ProfileId;

    private string? _infoMessage;

    private MalarkeyProfile? _profile;
    private IReadOnlyCollection<MalarkeyProfileIdentity> _identities = [];

    private IReadOnlyCollection<ProfileIdentityProviderEntry> _identityEntries = MalarkeyIdentityProviders.AllProviders
        .Select(prov => new ProfileIdentityProviderEntry(prov, []))
        .ToList();

    private readonly Guid _connectionState = Guid.NewGuid();



    private string? _intermediateName;
    private bool _useIntermediateName = false;
    public string? ProfileName { 
        get => _useIntermediateName ? _intermediateName : _profile?.ProfileName; 
        set {
            var newValue = value;
            if(ProfileId != null && newValue != null)
            {
                Task.Run(async () =>
                {
                    var saveResult = await ProfileRepo.UpdateProfileName(ProfileId.Value, newValue);
                    if (saveResult is SuccessActionResult<MalarkeyProfile> succ)
                    {
                        _useIntermediateName = false;
                        _profile = succ.Result;
                        _profileNameError = null;
                        _intermediateName = null;
                    }
                    else if(saveResult is ErrorActionResult<MalarkeyProfile> err)
                    {
                        _profileNameError = err.ErrorMessage;
                        _intermediateName = newValue;
                        _useIntermediateName = true;
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

    private string? _intermediateEmail;
    private bool _useIntermediateEmail = false;
    public string? PrimaryEmail
    {
        get => _useIntermediateEmail ? _intermediateEmail : _profile?.PrimaryEmail;
        set
        {
            if(!string.IsNullOrWhiteSpace(value) && !IsValidEmail(value))
            {
                _useIntermediateEmail = true;
                _intermediateEmail = value;
                _primaryEmailError = "Invalid email";
            }
            else
            {
                UpdateAndReload((repo, profId) => repo.UpdatePrimaryEmail(profId, value), "email");
                _primaryEmailError = null;
                _useIntermediateEmail = false;
                _intermediateEmail = null;
            }

        }
    }
    private string? _primaryEmailError;
    private bool IsValidEmail(string email) => MailAddress.TryCreate(email, out _);

    private bool CanSendVerifyEmail => !(_profile?.PrimaryEmailIsVerified ?? false) &&
        (_profile?.PrimaryEmail?.Pipe(IsValidEmail) ?? false) &&
        (_profile?.NextVerificationSendTime?.Pipe(tim => tim < DateTime.Now) ?? true);

    private string ProfileNameExtraClass => _profileNameError == null ? "" : "input-error";
    private string EmailClasses =>  "malarkey-profile-input" + " " + 
        (_primaryEmailError == null ? "" : "input-error");

    protected override async Task OnInitializedAsync()
    {
        if (_profile == null && SessionState.User != null)
        {
            var loaded = await ProfileRepo.LoadProfileAndIdentities(SessionState.User!.Profile.ProfileId);
            if(loaded != null)
            {
                _profile = loaded.Profile;
                _identities = loaded.Identities;
                var identityMap = _identities
                    .GroupBy(_ => _.IdentityProvider)
                    .ToDictionary(
                      _ => _.Key, 
                      _ => _.OrderBy(_ => _.EmailToUse ?? _.ProviderId)
                            .ToReadOnlyCollection()
                     );
                _identityEntries = MalarkeyIdentityProviders.AllProviders
                    .Select(prov => new ProfileIdentityProviderEntry(prov, identityMap.GetValueOrDefault(prov, [])))
                    .ToList();
            }
            await InvokeAsync(StateHasChanged);
        }
        if(!_hasRegisteredForEmailUpdates)
        {
            EmailHandler.OnUpdate += OnEmailVerification;
            _hasRegisteredForEmailUpdates = true;
        }
        if(!_hasRegisteredForIdentityRegistrationUpdates)
        {
            AuthenticationEvents.OnIdentificationRegistrationCompleted += OnIdentityRegistrationCompleted;
            _hasRegisteredForIdentityRegistrationUpdates = true;
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

    private void OnEmailVerification(object? sender, VerifiableEmail email)
    {
        if(ProfileId != null && email.ProfileId == ProfileId && email.EmailAddress.ToLower().Trim() == _profile?.PrimaryEmail?.ToLower()?.Trim())
        {
            Task.Run(async () =>
            {
                var reloaded = await ProfileRepo.LoadProfileAndIdentities(ProfileId.Value);
                if (reloaded != null)
                {
                    _profile = reloaded.Profile;
                    _identities = reloaded.Identities;
                    await InvokeAsync(StateHasChanged);
                }
            });
        }
    }

    private void OnSendVerificationEmailClicked()
    {
        if(ProfileId != null && _profile?.PrimaryEmail != null)
        {
            Task.Run(async () =>
            {
                await EmailHandler.SendVerificationMail(_profile.PrimaryEmail, ProfileId.Value);
            });

        }
    }

    private void OnIdentityRegistrationCompleted(object? sender, MalarkeyProfileIdentity identity)
    {
        if(ProfileId != null && identity.ProfileId == ProfileId)
        {
            _ = Task.Run(async () =>
            {
                var reloaded = await ProfileRepo.LoadProfileAndIdentities(ProfileId.Value);
                if (reloaded != null)
                {
                    _profile = reloaded.Profile;
                    _identities = reloaded.Identities;
                    await InvokeAsync(StateHasChanged);
                }
            });
        }
    }


    private void FlashInfo(string infoText)
    {
        _infoMessage = infoText;
        InvokeAsync(StateHasChanged);
        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            _infoMessage = null;
            await InvokeAsync(StateHasChanged);
        });
    }

    public void Dispose()
    {
        if(_hasRegisteredForEmailUpdates)
        {
            EmailHandler.OnUpdate -= OnEmailVerification;
            _hasRegisteredForEmailUpdates = false;
        }
        if(_hasRegisteredForIdentityRegistrationUpdates)
        {
            AuthenticationEvents.OnIdentificationRegistrationCompleted -= OnIdentityRegistrationCompleted;
            _hasRegisteredForIdentityRegistrationUpdates = false;
        }


    }
}