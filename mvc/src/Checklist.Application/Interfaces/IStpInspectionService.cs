using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IStpInspectionService
{
    Task<Result<StpDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StpAreaSummaryDto>>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StpResponsibleOptionDto>>> GetResponsibleOptionsAsync(CancellationToken cancellationToken = default);
    Task<Result> SaveAreaAsync(StpAreaUpsertDto request, CancellationToken cancellationToken = default);
    Task<Result<StpChecklistDraftDto>> GetChecklistDraftAsync(Guid? areaId, Guid? templateId, CancellationToken cancellationToken = default);
    Task<Result<StpChecklistResultDto>> SubmitChecklistAsync(StpChecklistSubmissionDto request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StpChecklistListItemDto>>> GetChecklistsAsync(StpChecklistListFiltersDto filters, CancellationToken cancellationToken = default);
    Task<Result<StpChecklistDetailsDto>> GetChecklistDetailsAsync(Guid checklistId, CancellationToken cancellationToken = default);
}
