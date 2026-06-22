using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Identity;

internal class UnavailableSupervisorAuthenticationService : ISupervisorAuthenticationService
{
    public Task<Result<SupervisorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<SupervisorSessionDto>.Fail(
            "A autenticacao manual depende da conexao com o banco do sistema e ela nao esta configurada."));
    }
}
