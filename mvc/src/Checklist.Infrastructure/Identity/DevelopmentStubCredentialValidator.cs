using Checklist.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Identity;

internal sealed class DevelopmentStubCredentialValidator : IActiveDirectoryCredentialValidator
{
    private readonly MvcAuthenticationOptions _authenticationOptions;

    public DevelopmentStubCredentialValidator(IOptions<MvcAuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public bool Validate(string login, string password)
    {
        var normalizedInput = SupervisorLoginNormalizer.Normalize(login);
        var normalizedConfigured = SupervisorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentUserName);

        return string.Equals(normalizedInput, normalizedConfigured, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, _authenticationOptions.DevelopmentPassword, StringComparison.Ordinal);
    }
}
