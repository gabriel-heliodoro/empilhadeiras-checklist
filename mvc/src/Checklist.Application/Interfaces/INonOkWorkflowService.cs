using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface INonOkWorkflowService
{
    Task<Result<IReadOnlyList<NonOkResponsibleOptionDto>>> ListResponsibleOptionsAsync(CancellationToken cancellationToken = default);
    Task<Result<NonOkPanelItemDto>> AssignAsync(Guid checklistItemId, NonOkAssignRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<NonOkPanelItemDto>> UpdateAsync(Guid checklistItemId, NonOkUpdateRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<NonOkPanelItemDto>> CompleteAsync(Guid checklistItemId, CancellationToken cancellationToken = default);
}
