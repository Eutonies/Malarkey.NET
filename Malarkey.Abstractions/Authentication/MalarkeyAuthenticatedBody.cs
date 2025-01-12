namespace Malarkey.Abstractions.Authentication;

public record MalarkeyAuthenticatedBody(
    string ProfileToken,
    string[] IdentityTokens
);