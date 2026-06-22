using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IChecklistReader
{
    Task<Result<ChecklistDetailsDto>> GetByIdAsync(Guid checklistId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChecklistListItemDto>>> ListAsync(
        ChecklistListFiltersDto filters,
        CancellationToken cancellationToken = default);
}
