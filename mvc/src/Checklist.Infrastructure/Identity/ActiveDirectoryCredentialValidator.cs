using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using Checklist.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Identity;

[SupportedOSPlatform("windows")]
internal sealed class ActiveDirectoryCredentialValidator : IActiveDirectoryCredentialValidator
{
    private readonly ActiveDirectoryOptions _options;

    public ActiveDirectoryCredentialValidator(IOptions<ActiveDirectoryOptions> options)
    {
        _options = options.Value;
    }

    public bool Validate(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(_options.Domain))
        {
            return false;
        }

        using var context = string.IsNullOrWhiteSpace(_options.Container)
            ? new PrincipalContext(ContextType.Domain, _options.Domain)
            : new PrincipalContext(ContextType.Domain, _options.Domain, _options.Container);

        return context.ValidateCredentials(login, password);
    }
}
