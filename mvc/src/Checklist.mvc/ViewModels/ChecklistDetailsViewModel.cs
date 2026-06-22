namespace Checklist.Mvc.ViewModels;

public class ChecklistDetailsViewModel
{
    public Guid Id { get; init; }
    public required string Code { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string SectorName { get; init; }
    public required string Status { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public IReadOnlyList<ChecklistItemViewModel> Items { get; init; } = [];
}
