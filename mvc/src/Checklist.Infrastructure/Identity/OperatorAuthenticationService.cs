using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

internal class OperatorAuthenticationService : IOperatorAuthenticationService
{
    private readonly AppDbContext _dbContext;

    public OperatorAuthenticationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OperatorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedLogin = OperatorLoginNormalizer.Normalize(login);
        var normalizedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedLogin) || string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return Result<OperatorSessionDto>.Fail("Login e senha sao obrigatorios.");
        }

        var operador = await _dbContext.Operators
            .AsTracking()
            .Include(x => x.Sector)
            .FirstOrDefaultAsync(x => x.Login == normalizedLogin && x.IsActive, cancellationToken);

        if (operador is null || !ActiveDirectoryService.AuthenticateAD(normalizedLogin, normalizedPassword))
        {
            return Result<OperatorSessionDto>.Fail("Login ou senha invalidos.");
        }

        if (!operador.Sector.IsActive)
        {
            return Result<OperatorSessionDto>.Fail("O setor deste operador esta inativo.");
        }

        operador.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<OperatorSessionDto>.Ok(MapSession(operador));
    }

    private static OperatorSessionDto MapSession(Data.Models.MvcOperator operador)
    {
        return new OperatorSessionDto
        {
            Id = operador.Id,
            SectorId = operador.SectorId,
            Name = $"{operador.Name} {operador.LastName}".Trim(),
            Registration = operador.Registration,
            Login = operador.Login,
            SectorName = operador.Sector.Name,
            ForceChangePassword = false
        };
    }
}
