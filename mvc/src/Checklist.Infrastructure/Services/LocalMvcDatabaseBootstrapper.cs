using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Checklist.Infrastructure.Identity;
using Checklist.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Services;

public class LocalMvcDatabaseBootstrapper
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;
    private readonly MvcAuthenticationOptions _authenticationOptions;
    private readonly MasterAccountOptions _masterAccountOptions;

    public LocalMvcDatabaseBootstrapper(
        AppDbContext dbContext,
        PasswordHashingService passwordHashingService,
        IOptions<MvcAuthenticationOptions> authenticationOptions,
        IOptions<MasterAccountOptions> masterAccountOptions)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _authenticationOptions = authenticationOptions.Value;
        _masterAccountOptions = masterAccountOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        
        if (_dbContext.Database.IsSqlServer())
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        
        await EnsureMasterAccountAsync(cancellationToken);

        if (!Guid.TryParse(_authenticationOptions.DevelopmentSectorId, out var supervisorSectorId))
        {
            return;
        }

        var operatorSectorId = Guid.TryParse(_authenticationOptions.DevelopmentOperatorSectorId, out var parsedOperatorSectorId)
            ? parsedOperatorSectorId
            : supervisorSectorId;

        await EnsureSectorAsync(
            supervisorSectorId,
            "SCE - Expedicao",
            "Setor bootstrap local para supervisao operacional.",
            cancellationToken);

        if (operatorSectorId != supervisorSectorId)
        {
            await EnsureSectorAsync(
                operatorSectorId,
                _authenticationOptions.DevelopmentOperatorSectorName,
                "Setor bootstrap local para fluxo do operador.",
                cancellationToken);
        }

        if (string.Equals(_authenticationOptions.Mode, MvcAuthenticationOptions.DevelopmentStubMode, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(_authenticationOptions.DevelopmentUserId, out var supervisorUserId))
        {
            await EnsureDevelopmentSupervisorAsync(supervisorUserId, supervisorSectorId, cancellationToken);
        }

        if (string.Equals(_authenticationOptions.Mode, MvcAuthenticationOptions.DevelopmentStubMode, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(_authenticationOptions.DevelopmentOperatorId, out var operatorId))
        {
            await EnsureOperatorAsync(operatorId, operatorSectorId, cancellationToken);
        }
    }

    private async Task EnsureSectorAsync(
        Guid sectorId,
        string? sectorName,
        string description,
        CancellationToken cancellationToken)
    {
        var sector = await _dbContext.Sectors.FirstOrDefaultAsync(x => x.Id == sectorId, cancellationToken);
        if (sector is null)
        {
            sector = new MvcSector
            {
                Id = sectorId,
                Name = string.IsNullOrWhiteSpace(sectorName) ? "Setor local" : sectorName.Trim(),
                Description = description,
                IsActive = true
            };

            _dbContext.Sectors.Add(sector);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!sector.IsActive)
        {
            sector.IsActive = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureMasterAccountAsync(CancellationToken cancellationToken)
    {
        var login = SupervisorLoginNormalizer.Normalize(_masterAccountOptions.Login ?? string.Empty);
        var password = (_masterAccountOptions.Password ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var masterExists = await _dbContext.SupervisorUsers.AnyAsync(x => x.IsMaster, cancellationToken);
        if (masterExists)
        {
            return;
        }

        var sectorName = string.IsNullOrWhiteSpace(_masterAccountOptions.SectorName)
            ? "Administracao"
            : _masterAccountOptions.SectorName.Trim();

        var sector = await _dbContext.Sectors.FirstOrDefaultAsync(x => x.Name == sectorName, cancellationToken);
        if (sector is null)
        {
            sector = new MvcSector
            {
                Name = sectorName,
                Description = "Setor administrativo da conta master de bootstrap.",
                IsActive = true
            };

            _dbContext.Sectors.Add(sector);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var master = new MvcSupervisorUser
        {
            Name = string.IsNullOrWhiteSpace(_masterAccountOptions.Name) ? "Admin" : _masterAccountOptions.Name.Trim(),
            LastName = string.IsNullOrWhiteSpace(_masterAccountOptions.LastName) ? "Master" : _masterAccountOptions.LastName.Trim(),
            Login = login,
            PasswordHash = _passwordHashingService.HashPassword(password),
            ForceChangePassword = false,
            IsMaster = true,
            UserType = MvcUserAccessType.Supervisor,
            SectorId = sector.Id,
            IsActive = true
        };

        _dbContext.SupervisorUsers.Add(master);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureOperatorAsync(Guid operatorId, Guid sectorId, CancellationToken cancellationToken)
    {
        var login = OperatorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentOperatorUserName);
        var registration = (_authenticationOptions.DevelopmentOperatorRegistration ?? string.Empty).Trim();
        var fullName = string.IsNullOrWhiteSpace(_authenticationOptions.DevelopmentOperatorName)
            ? "Operador local"
            : _authenticationOptions.DevelopmentOperatorName.Trim();
        var (firstName, lastName) = SplitOperatorName(fullName);

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(registration))
        {
            return;
        }

        var op = await _dbContext.Operators.FirstOrDefaultAsync(x => x.Id == operatorId, cancellationToken);
        if (op is null)
        {
            op = new MvcOperator
            {
                Id = operatorId,
                SectorId = sectorId,
                Registration = registration,
                Name = firstName,
                LastName = lastName,
                Login = login,
                PasswordHash = _passwordHashingService.HashPassword(_authenticationOptions.DevelopmentOperatorPassword),
                ForceChangePassword = _authenticationOptions.DevelopmentOperatorForceChangePassword,
                IsActive = true
            };

            _dbContext.Operators.Add(op);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        op.SectorId = sectorId;
        op.Registration = registration;
        op.Name = firstName;
        op.LastName = lastName;
        op.Login = login;
        op.PasswordHash = _passwordHashingService.HashPassword(_authenticationOptions.DevelopmentOperatorPassword);
        op.ForceChangePassword = _authenticationOptions.DevelopmentOperatorForceChangePassword;
        op.IsActive = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDevelopmentSupervisorAsync(Guid supervisorUserId, Guid sectorId, CancellationToken cancellationToken)
    {
        var login = SupervisorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentUserName);
        var password = (_authenticationOptions.DevelopmentPassword ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var (firstName, lastName) = SplitDisplayName(_authenticationOptions.DevelopmentUserName);
        var isMaster = _authenticationOptions.DevelopmentIsMaster;
        var userType = string.Equals(_authenticationOptions.DevelopmentUserType, "Inspector", StringComparison.OrdinalIgnoreCase)
            ? MvcUserAccessType.Inspector
            : MvcUserAccessType.Supervisor;
        var modules = MapModuleCodes(_authenticationOptions.DevelopmentModuleCodes, userType);

        var user = await _dbContext.SupervisorUsers
            .FirstOrDefaultAsync(x => x.Id == supervisorUserId, cancellationToken);

        if (user is null)
        {
            user = new MvcSupervisorUser
            {
                Id = supervisorUserId,
                SectorId = sectorId,
                Name = firstName,
                LastName = lastName,
                Login = login,
                PasswordHash = _passwordHashingService.HashPassword(password),
                ForceChangePassword = _authenticationOptions.DevelopmentForceChangePassword,
                IsMaster = isMaster,
                UserType = userType,
                IsActive = true
            };

            user.Modules = modules
                .Select(module => new MvcSupervisorUserModule
                {
                    SupervisorUserId = user.Id,
                    Module = module
                })
                .ToList();

            _dbContext.SupervisorUsers.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        user.SectorId = sectorId;
        user.Name = firstName;
        user.LastName = lastName;
        user.Login = login;
        user.PasswordHash = _passwordHashingService.HashPassword(password);
        user.ForceChangePassword = _authenticationOptions.DevelopmentForceChangePassword;
        user.IsMaster = isMaster;
        user.UserType = userType;
        user.IsActive = true;

        await _dbContext.SupervisorUserModules
            .Where(x => x.SupervisorUserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.SupervisorUserModules.AddRange(modules.Select(module => new MvcSupervisorUserModule
        {
            SupervisorUserId = user.Id,
            Module = module
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static (string FirstName, string LastName) SplitOperatorName(string fullName)
    {
        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return ("Operador", "Local");
        }

        if (parts.Length == 1)
        {
            return (parts[0], "Operador");
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static (string FirstName, string LastName) SplitDisplayName(string value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "Inspector Seguranca" : value.Trim();
        var normalized = source
            .Replace('.', ' ')
            .Replace('-', ' ')
            .Replace('_', ' ');

        var parts = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 2)
        {
            return (parts[0], string.Join(' ', parts.Skip(1)));
        }

        var compact = SupervisorLoginNormalizer.Normalize(source);
        var pascalParts = new List<string>();
        var current = new List<char>();

        foreach (var ch in compact)
        {
            if (current.Count > 0 && char.IsUpper(ch))
            {
                pascalParts.Add(new string(current.ToArray()));
                current.Clear();
            }

            current.Add(ch);
        }

        if (current.Count > 0)
        {
            pascalParts.Add(new string(current.ToArray()));
        }

        if (pascalParts.Count >= 2)
        {
            return (pascalParts[0], string.Join(' ', pascalParts.Skip(1)));
        }

        return (source, "Local");
    }

    private static List<MvcAccessModule> MapModuleCodes(IEnumerable<string>? moduleCodes, MvcUserAccessType userType)
    {
        var modules = new List<MvcAccessModule>();

        foreach (var moduleCode in moduleCodes ?? [])
        {
            if (string.Equals(moduleCode, "operational-supervision", StringComparison.OrdinalIgnoreCase))
            {
                modules.Add(MvcAccessModule.OperationalSupervision);
                continue;
            }

            if (string.Equals(moduleCode, "work-safety", StringComparison.OrdinalIgnoreCase))
            {
                modules.Add(MvcAccessModule.WorkSafety);
                continue;
            }

            if (string.Equals(moduleCode, "material-inspection", StringComparison.OrdinalIgnoreCase))
            {
                modules.Add(MvcAccessModule.MaterialInspection);
            }
        }

        if (modules.Count == 0)
        {
            modules.Add(userType == MvcUserAccessType.Inspector
                ? MvcAccessModule.WorkSafety
                : MvcAccessModule.OperationalSupervision);
        }

        return modules.Distinct().ToList();
    }
}
