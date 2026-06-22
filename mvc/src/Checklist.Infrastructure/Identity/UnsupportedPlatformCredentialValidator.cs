namespace Checklist.Infrastructure.Identity;

internal sealed class UnsupportedPlatformCredentialValidator : IActiveDirectoryCredentialValidator
{
    public bool Validate(string login, string password)
    {
        return false;
    }
}
