namespace Checklist.Mvc.ViewModels;

public class DashboardViewModel
{
    public required string UserDisplayName { get; init; }
    public required string CurrentUtcTimestamp { get; init; }
    public int ChecklistCount { get; init; }
    public int EquipmentCount { get; init; }
    public Guid? SampleChecklistId { get; init; }
    public string? SampleChecklistCode { get; init; }
    public IReadOnlyList<DashboardEquipmentGroupViewModel> EquipmentGroups { get; init; } = [];
}
