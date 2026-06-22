using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface INonOkReader
{
    Task<Result<NonOkPanelDto>> GetPanelAsync(NonOkFiltersDto filters, CancellationToken cancellationToken = default);
    Task<Result<NonOkPanelItemDto>> GetByIdAsync(Guid checklistItemId, CancellationToken cancellationToken = default);
}
