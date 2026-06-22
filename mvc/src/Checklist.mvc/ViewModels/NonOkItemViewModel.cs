namespace Checklist.Mvc.ViewModels;

public class NonOkItemViewModel
{
    public Guid ChecklistId { get; init; }
    public Guid ChecklistItemId { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string OperatorRegistration { get; init; }
    public required string SectorName { get; init; }
    public int Order { get; init; }
    public required string Description { get; init; }
    public string? Instruction { get; init; }
    public string? Observation { get; init; }
    public required string WorkflowStatus { get; init; }
    public required string WorkflowLabel { get; init; }
    public required string WorkflowCssClass { get; init; }
    public string? ResponsibleName { get; init; }
    public string? ResponsibleSectorName { get; init; }
    public int CompletionPercent { get; init; }
    public string? PlannedCompletionDateDisplay { get; init; }
    public required string ChecklistDateDisplay { get; init; }
}
