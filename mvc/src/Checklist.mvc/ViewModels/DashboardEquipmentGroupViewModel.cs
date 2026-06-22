namespace Checklist.Mvc.ViewModels;

public class DashboardEquipmentGroupViewModel
{
    public required string CategoryName { get; init; }
    public int EquipmentCount { get; init; }
    public IReadOnlyList<DashboardEquipmentStatusViewModel> Equipments { get; init; } = [];
}
