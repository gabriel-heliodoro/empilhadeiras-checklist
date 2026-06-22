using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

internal class UnavailableStpInspectionService : IStpInspectionService
{
    private const string Message = "STP indisponivel sem conexao com banco de dados.";

    public Task<Result<StpDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpDashboardDto>.Fail(Message));

    public Task<Result<IReadOnlyList<StpAreaSummaryDto>>> GetAreasAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<IReadOnlyList<StpAreaSummaryDto>>.Fail(Message));

    public Task<Result<IReadOnlyList<StpResponsibleOptionDto>>> GetResponsibleOptionsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<IReadOnlyList<StpResponsibleOptionDto>>.Fail(Message));

    public Task<Result> SaveAreaAsync(StpAreaUpsertDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Fail(Message));

    public Task<Result<StpChecklistDraftDto>> GetChecklistDraftAsync(Guid? areaId, Guid? templateId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpChecklistDraftDto>.Fail(Message));

    public Task<Result<StpChecklistResultDto>> SubmitChecklistAsync(StpChecklistSubmissionDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpChecklistResultDto>.Fail(Message));

    public Task<Result<IReadOnlyList<StpChecklistListItemDto>>> GetChecklistsAsync(StpChecklistListFiltersDto filters, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<IReadOnlyList<StpChecklistListItemDto>>.Fail(Message));

    public Task<Result<StpChecklistDetailsDto>> GetChecklistDetailsAsync(Guid checklistId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpChecklistDetailsDto>.Fail(Message));
}
