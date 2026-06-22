using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

internal class SupervisorAuthenticationService : ISupervisorAuthenticationService
{
    private readonly AppDbContext _db;
    private readonly IActiveDirectoryCredentialValidator _credentialValidator;

    public SupervisorAuthenticationService(
        AppDbContext db,
        IActiveDirectoryCredentialValidator credentialValidator)
    {
        _db = db;
        _credentialValidator = credentialValidator;
    }

    public async Task<Result<SupervisorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedLogin = SupervisorLoginNormalizer.Normalize(login);

        if (string.IsNullOrWhiteSpace(normalizedLogin) || string.IsNullOrWhiteSpace(password))
        {
            return Result<SupervisorSessionDto>.Fail("Login e senha sao obrigatorios.");
        }

        if (!_credentialValidator.Validate(normalizedLogin, password))
        {
            return Result<SupervisorSessionDto>.Fail("Login ou senha invalidos.");
        }

        var supervisor = await _db.SupervisorUsers
            .AsNoTracking()
            .Include(x => x.Sector)
            .Include(x => x.Modules)
            .FirstOrDefaultAsync(x => x.Login == normalizedLogin && x.IsActive, cancellationToken);

        if (supervisor is null)
        {
            return Result<SupervisorSessionDto>.Fail("Supervisor nao encontrado ou inativo.");
        }

        if (!supervisor.IsMaster && !supervisor.Sector.IsActive)
        {
            return Result<SupervisorSessionDto>.Fail("O setor deste supervisor esta inativo.");
        }

        return Result<SupervisorSessionDto>.Ok(new SupervisorSessionDto
        {
            Id = supervisor.Id,
            SectorId = supervisor.SectorId,
            Login = supervisor.Login,
            DisplayName = $"{supervisor.Name} {supervisor.LastName}".Trim(),
            ForceChangePassword = supervisor.ForceChangePassword,
            IsMaster = supervisor.IsMaster,
            UserType = supervisor.UserType.ToString(),
            ModuleCodes = supervisor.IsMaster
                ? []
                : supervisor.Modules
                    .Select(x => AccessModuleMapper.ToCode(x.Module))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()
        });
    }
}
