namespace Checklist.Mvc.ViewModels;

public class OperatorEquipmentSearchViewModel
{
    public string Query { get; set; } = string.Empty;
    public string OperatorName { get; init; } = string.Empty;
    public string OperatorRegistration { get; init; } = string.Empty;
    public string SectorName { get; init; } = string.Empty;
    public bool ForceChangePassword { get; init; }
    public IReadOnlyList<OperatorEquipmentSearchItemViewModel> Results { get; init; } = [];
}
