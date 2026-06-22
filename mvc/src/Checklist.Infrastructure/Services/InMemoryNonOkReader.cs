using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

public class InMemoryNonOkReader : INonOkReader
{
    public Task<Result<NonOkPanelItemDto>> GetByIdAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        var item = SampleChecklistStore.NonOkItemState.FirstOrDefault(entry => entry.ChecklistItemId == checklistItemId);
        if (item is null)
        {
            return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Item non-compliant nao encontrado."));
        }

        return Task.FromResult(Result<NonOkPanelItemDto>.Ok(item));
    }

    public Task<Result<NonOkPanelDto>> GetPanelAsync(NonOkFiltersDto filters, CancellationToken cancellationToken = default)
    {
        IEnumerable<NonOkPanelItemDto> query = SampleChecklistStore.NonOkItemState;

        if (!string.IsNullOrWhiteSpace(filters.Equipment))
        {
            query = query.Where(item =>
                item.EquipmentCode.Contains(filters.Equipment, StringComparison.OrdinalIgnoreCase) ||
                item.EquipmentDescription.Contains(filters.Equipment, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Operator))
        {
            query = query.Where(item =>
                item.OperatorName.Contains(filters.Operator, StringComparison.OrdinalIgnoreCase) ||
                item.OperatorRegistration.Contains(filters.Operator, StringComparison.OrdinalIgnoreCase));
        }

        if (DateTime.TryParse(filters.DataInicio, out var dataInicio))
        {
            var inicioUtc = new DateTime(dataInicio.Year, dataInicio.Month, dataInicio.Day, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(item => item.ChecklistCompletedAt >= inicioUtc);
        }

        if (DateTime.TryParse(filters.DataFim, out var dataFim))
        {
            var fimUtc = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            query = query.Where(item => item.ChecklistCompletedAt < fimUtc);
        }

        var items = query.OrderByDescending(item => item.ChecklistCompletedAt).ToList();

        var dto = new NonOkPanelDto
        {
            PendingApproval = items.Where(item => item.WorkflowStatus == "pending-approval").ToList(),
            InProgress = items.Where(item => item.WorkflowStatus == "in-progress").ToList(),
            Completed = items.Where(item => item.WorkflowStatus == "completed").ToList()
        };

        return Task.FromResult(Result<NonOkPanelDto>.Ok(dto));
    }
}

