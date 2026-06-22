using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Identity;

internal class OperatorAuthenticationService : IOperatorAuthenticationService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHashingService _passwordHashingService;

    public OperatorAuthenticationService(
        AppDbContext dbContext,
        PasswordHashingService passwordHashingService)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
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

        if (operador is null || !_passwordHashingService.VerifyPassword(normalizedPassword, operador.PasswordHash))
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

    public async Task<Result<OperatorSessionDto>> ChangePasswordAsync(
        Guid operatorId,
        string newPassword,
        string confirmationPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedPassword = newPassword?.Trim() ?? string.Empty;
        var normalizedConfirmation = confirmationPassword?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return Result<OperatorSessionDto>.Fail("Nova senha e obrigatoria.");
        }

        if (!string.Equals(normalizedPassword, normalizedConfirmation, StringComparison.Ordinal))
        {
            return Result<OperatorSessionDto>.Fail("Nova senha e confirmacao precisam ser iguais.");
        }

        if (normalizedPassword.Length < 8)
        {
            return Result<OperatorSessionDto>.Fail("A nova senha precisa ter pelo menos 8 caracteres.");
        }

        var operador = await _dbContext.Operators
            .AsTracking()
            .Include(x => x.Sector)
            .FirstOrDefaultAsync(x => x.Id == operatorId && x.IsActive, cancellationToken);

        if (operador is null || !operador.Sector.IsActive)
        {
            return Result<OperatorSessionDto>.Fail("Operator nao encontrado ou inativo.");
        }

        operador.PasswordHash = _passwordHashingService.HashPassword(normalizedPassword);
        operador.ForceChangePassword = false;
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
            Name = operador.Name,
            Registration = operador.Registration,
            Login = operador.Login,
            SectorName = operador.Sector.Name,
            ForceChangePassword = operador.ForceChangePassword
        };
    }
}
