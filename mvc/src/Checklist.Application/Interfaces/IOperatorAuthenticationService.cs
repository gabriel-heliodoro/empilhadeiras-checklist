using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IOperatorAuthenticationService
{
    Task<Result<OperatorSessionDto>> AuthenticateAsync(string login, string password, CancellationToken cancellationToken = default);
    Task<Result<OperatorSessionDto>> ChangePasswordAsync(Guid operatorId, string newPassword, string confirmationPassword, CancellationToken cancellationToken = default);
}
