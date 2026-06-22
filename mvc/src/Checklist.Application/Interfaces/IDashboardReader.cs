using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IDashboardReader
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
