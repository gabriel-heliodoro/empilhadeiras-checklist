namespace Checklist.Infrastructure.Identity;

internal interface IActiveDirectoryCredentialValidator
{
    bool Validate(string login, string password);
}
