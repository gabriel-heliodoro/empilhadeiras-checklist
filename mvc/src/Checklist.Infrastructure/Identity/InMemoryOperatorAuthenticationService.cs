using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Checklist.Infrastructure.Identity;

internal class InMemoryOperatorAuthenticationService : IOperatorAuthenticationService
{
    private static readonly object Sync = new();
    private static InMemoryOperatorState? _state;

    private readonly MvcAuthenticationOptions _authenticationOptions;

    public InMemoryOperatorAuthenticationService(IOptions<MvcAuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public Task<Result<OperatorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var state = EnsureState();
        var normalizedLogin = OperatorLoginNormalizer.Normalize(login);
        var normalizedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedLogin) || string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("Login e senha sao obrigatorios."));
        }

        if (!string.Equals(normalizedLogin, state.Login, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(normalizedPassword, state.Password, StringComparison.Ordinal))
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("Login ou senha invalidos."));
        }

        return Task.FromResult(Result<OperatorSessionDto>.Ok(state.ToSession()));
    }

    public Task<Result<OperatorSessionDto>> ChangePasswordAsync(
        Guid operatorId,
        string newPassword,
        string confirmationPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedPassword = newPassword?.Trim() ?? string.Empty;
        var normalizedConfirmation = confirmationPassword?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("Nova senha e obrigatoria."));
        }

        if (!string.Equals(normalizedPassword, normalizedConfirmation, StringComparison.Ordinal))
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("Nova senha e confirmacao precisam ser iguais."));
        }

        if (normalizedPassword.Length < 8)
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("A nova senha precisa ter pelo menos 8 caracteres."));
        }

        var state = EnsureState();
        if (state.Id != operatorId)
        {
            return Task.FromResult(Result<OperatorSessionDto>.Fail("Operator autenticado invalido."));
        }

        lock (Sync)
        {
            state.Password = normalizedPassword;
            state.ForceChangePassword = false;
        }

        return Task.FromResult(Result<OperatorSessionDto>.Ok(state.ToSession()));
    }

    private InMemoryOperatorState EnsureState()
    {
        if (_state is not null)
        {
            return _state;
        }

        lock (Sync)
        {
            if (_state is not null)
            {
                return _state;
            }

            if (!Guid.TryParse(_authenticationOptions.DevelopmentOperatorId, out var operatorId)
                || !Guid.TryParse(_authenticationOptions.DevelopmentOperatorSectorId, out var setorId))
            {
                throw new InvalidOperationException("Os dados do operador de desenvolvimento estao invalidos na configuracao.");
            }

            _state = new InMemoryOperatorState
            {
                Id = operatorId,
                SectorId = setorId,
                Name = _authenticationOptions.DevelopmentOperatorName,
                Registration = _authenticationOptions.DevelopmentOperatorRegistration,
                Login = OperatorLoginNormalizer.Normalize(_authenticationOptions.DevelopmentOperatorUserName),
                Password = _authenticationOptions.DevelopmentOperatorPassword,
                SectorName = _authenticationOptions.DevelopmentOperatorSectorName,
                ForceChangePassword = _authenticationOptions.DevelopmentOperatorForceChangePassword
            };

            return _state;
        }
    }

    private sealed class InMemoryOperatorState
    {
        public Guid Id { get; init; }
        public Guid SectorId { get; init; }
        public required string Name { get; init; }
        public required string Registration { get; init; }
        public required string Login { get; init; }
        public required string Password { get; set; }
        public required string SectorName { get; init; }
        public bool ForceChangePassword { get; set; }

        public OperatorSessionDto ToSession()
        {
            return new OperatorSessionDto
            {
                Id = Id,
                SectorId = SectorId,
                Name = Name,
                Registration = Registration,
                Login = Login,
                SectorName = SectorName,
                ForceChangePassword = ForceChangePassword
            };
        }
    }
}
