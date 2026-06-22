using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Checklist.Infrastructure.Identity;
using Checklist.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Services;

public class LocalMvcDatabaseBootstrapper
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;
    private readonly MvcAuthenticationOptions _authenticationOptions;

    public LocalMvcDatabaseBootstrapper(
        AppDbContext dbContext,
        PasswordHashingService passwordHashingService,
        IOptions<MvcAuthenticationOptions> authenticationOptions)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _authenticationOptions = authenticationOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

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

        if (Guid.TryParse(_authenticationOptions.DevelopmentOperatorId, out var operatorId))
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

    private async Task EnsureOperatorAsync(Guid operatorId, Guid sectorId, CancellationToken cancellationToken)
    {
        var login = OperatorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentOperatorUserName);
        var registration = (_authenticationOptions.DevelopmentOperatorRegistration ?? string.Empty).Trim();
        var name = string.IsNullOrWhiteSpace(_authenticationOptions.DevelopmentOperatorName)
            ? "Operador local"
            : _authenticationOptions.DevelopmentOperatorName.Trim();

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
                Name = name,
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
        op.Name = name;
        op.Login = login;
        op.PasswordHash = _passwordHashingService.HashPassword(_authenticationOptions.DevelopmentOperatorPassword);
        op.ForceChangePassword = _authenticationOptions.DevelopmentOperatorForceChangePassword;
        op.IsActive = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
