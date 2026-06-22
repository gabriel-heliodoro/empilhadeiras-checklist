using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

public class InMemoryChecklistReader : IChecklistReader
{
    public Task<Result<ChecklistDetailsDto>> GetByIdAsync(Guid checklistId, CancellationToken cancellationToken = default)
    {
        var checklist = SampleChecklistStore.GetChecklist(checklistId);
        if (checklist is null)
        {
            return Task.FromResult(Result<ChecklistDetailsDto>.Fail("Checklist de teste nao encontrado."));
        }

        return Task.FromResult(Result<ChecklistDetailsDto>.Ok(checklist));
    }

    public Task<Result<IReadOnlyList<ChecklistListItemDto>>> ListAsync(
        ChecklistListFiltersDto filters,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<ChecklistListItemDto> query = SampleChecklistStore.GetChecklists();

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            query = query.Where(item => string.Equals(item.Status, filters.Status, StringComparison.OrdinalIgnoreCase));
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
            query = query.Where(item => item.CreatedAt >= inicioUtc);
        }

        if (DateTime.TryParse(filters.DataFim, out var dataFim))
        {
            var fimUtc = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            query = query.Where(item => item.CreatedAt < fimUtc);
        }

        var result = query
            .OrderByDescending(item => item.CreatedAt)
            .ToList()
            .AsReadOnly();

        return Task.FromResult(Result<IReadOnlyList<ChecklistListItemDto>>.Ok(result));
    }
}
