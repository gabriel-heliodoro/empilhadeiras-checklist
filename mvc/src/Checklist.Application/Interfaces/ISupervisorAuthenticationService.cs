using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface ISupervisorAuthenticationService
{
    Task<Result<SupervisorSessionDto>> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default);
}
