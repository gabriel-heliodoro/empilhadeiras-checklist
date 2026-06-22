using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Identity;

internal class DevelopmentSupervisorAuthenticationService : ISupervisorAuthenticationService
{
    private readonly IActiveDirectoryCredentialValidator _credentialValidator;
    private readonly MvcAuthenticationOptions _authenticationOptions;

    public DevelopmentSupervisorAuthenticationService(
        IActiveDirectoryCredentialValidator credentialValidator,
        IOptions<MvcAuthenticationOptions> authenticationOptions)
    {
        _credentialValidator = credentialValidator;
        _authenticationOptions = authenticationOptions.Value;
    }

    public Task<Result<SupervisorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedLogin = SupervisorLoginNormalizer.Normalize(login);

        if (string.IsNullOrWhiteSpace(normalizedLogin) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(Result<SupervisorSessionDto>.Fail("Login e senha sao obrigatorios."));
        }

        if (!_credentialValidator.Validate(normalizedLogin, password))
        {
            return Task.FromResult(Result<SupervisorSessionDto>.Fail("Login ou senha invalidos."));
        }

        if (!Guid.TryParse(_authenticationOptions.DevelopmentUserId, out var userId)
            || !Guid.TryParse(_authenticationOptions.DevelopmentSectorId, out var setorId))
        {
            return Task.FromResult(Result<SupervisorSessionDto>.Fail(
                "Os dados do usuario de desenvolvimento estao invalidos na configuracao."));
        }

        return Task.FromResult(Result<SupervisorSessionDto>.Ok(new SupervisorSessionDto
        {
            Id = userId,
            SectorId = setorId,
            Login = SupervisorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentUserName),
            DisplayName = _authenticationOptions.DevelopmentUserName,
            ForceChangePassword = _authenticationOptions.DevelopmentForceChangePassword,
            IsMaster = _authenticationOptions.DevelopmentIsMaster,
            UserType = _authenticationOptions.DevelopmentUserType,
            ModuleCodes = _authenticationOptions.DevelopmentModuleCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(code => code)
                .ToList()
        }));
    }
}
