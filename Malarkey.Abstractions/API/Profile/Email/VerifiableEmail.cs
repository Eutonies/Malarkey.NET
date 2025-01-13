namespace Malarkey.Abstractions.API.Profile.Email;

public record VerifiableEmail(
    long EmailAddressId,
    Guid ProfileId,
    string EmailAddress,
    string CodeString,
    DateTime? LastVerificationEmailSent,
    DateTime? VerifiedAt)
{
    public const int MinHoursBetweenSends = 6;

    private VerifiableEmailStatus? _status;
    public VerifiableEmailStatus Status => _status ??= ((VerifiedAt, EmailAddressId) switch
    {
        (_, 0L) => VerifiableEmailStatus.NotRegistered,
        (null, _) when LastVerificationEmailSent != null => VerifiableEmailStatus.EmailSent,
        (DateTime verd, _) => VerifiableEmailStatus.EmailVerified,
        _ => VerifiableEmailStatus.Registered
    });

    public DateTime NextCanSendTime => LastVerificationEmailSent?.AddHours(MinHoursBetweenSends) ?? DateTime.MinValue;


}
