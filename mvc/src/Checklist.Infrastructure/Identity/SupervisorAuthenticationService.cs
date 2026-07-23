using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

internal class SupervisorAuthenticationService : ISupervisorAuthenticationService
{
    private const string MasterRoleName = "Master";

    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public SupervisorAuthenticationService(
        AppDbContext db,
        UserManager<IdentityUser<Guid>> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<Result<SupervisorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedLogin = SupervisorLoginNormalizer.Normalize(login);
        var normalizedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedLogin) || string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return Result<SupervisorSessionDto>.Fail("Login e senha sao obrigatorios.");
        }

        if (!ActiveDirectoryService.AuthenticateAD(normalizedLogin, normalizedPassword))
        {
            return Result<SupervisorSessionDto>.Fail("Login ou senha invalidos.");
        }

        var identityUser = await _userManager.FindByNameAsync(normalizedLogin);
        if (identityUser is not null && await _userManager.IsInRoleAsync(identityUser, MasterRoleName))
        {
            return Result<SupervisorSessionDto>.Ok(new SupervisorSessionDto
            {
                Id = identityUser.Id,
                SectorId = Guid.Empty,
                Login = normalizedLogin,
                DisplayName = identityUser.UserName ?? normalizedLogin,
                ForceChangePassword = false,
                IsMaster = true,
                UserType = "Supervisor",
                ModuleCodes = []
            });
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

        if (!supervisor.Sector.IsActive)
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
            IsMaster = false,
            UserType = supervisor.UserType.ToString(),
            ModuleCodes = supervisor.Modules
                .Select(x => AccessModuleMapper.ToCode(x.Module))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList()
        });
    }
}
