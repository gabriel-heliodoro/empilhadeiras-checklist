namespace Checklist.Mvc.ViewModels;

public class DashboardEquipmentStatusViewModel
{
    public Guid EquipmentId { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public string? CategoryName { get; init; }
    public required string StatusKey { get; init; }
    public required string StatusLabel { get; init; }
    public required string StatusCssClass { get; init; }
    public Guid? ChecklistId { get; init; }
    public string? ChecklistCompletedAtDisplay { get; init; }
}
