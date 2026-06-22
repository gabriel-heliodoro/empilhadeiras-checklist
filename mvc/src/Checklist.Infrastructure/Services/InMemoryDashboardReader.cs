using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

public class InMemoryDashboardReader : IDashboardReader
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public InMemoryDashboardReader(ICurrentUser currentUser, IDateTimeProvider dateTimeProvider)
    {
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var equipments = SampleChecklistStore.GetDashboardEquipments();
        var latestChecklist = SampleChecklistStore.GetChecklists().FirstOrDefault();
        var latestChecklistCode = latestChecklist is null
            ? null
            : SampleChecklistStore.GetChecklist(latestChecklist.Id)?.Code;

        var dto = new DashboardSummaryDto
        {
            UserDisplayName = _currentUser.UserName ?? "Supervisor de teste",
            CurrentUtcTimestamp = _dateTimeProvider.CurrentUtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            ChecklistCount = equipments.Count(x => x.ChecklistId.HasValue),
            EquipmentCount = equipments.Count,
            SampleChecklistId = latestChecklist?.Id,
            SampleChecklistCode = latestChecklistCode,
            Equipments = equipments
        };

        return Task.FromResult(Result<DashboardSummaryDto>.Ok(dto));
    }
}
