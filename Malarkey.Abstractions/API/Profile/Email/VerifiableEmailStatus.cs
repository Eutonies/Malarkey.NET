namespace Malarkey.Abstractions.API.Profile.Email;

public enum VerifiableEmailStatus
{
    InvalidEmail = 1,
    NotRegistered = 10,
    Registered = 20,
    EmailSent = 30,
    EmailVerified = 100

}
