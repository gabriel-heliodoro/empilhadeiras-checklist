namespace Checklist.Mvc.ViewModels;

public class NonOkDetailsViewModel
{
    public Guid ChecklistItemId { get; init; }
    public Guid ChecklistId { get; init; }
    public required string WorkflowStatus { get; init; }
    public required string WorkflowLabel { get; init; }
    public required string WorkflowCssClass { get; init; }
    public required string ChecklistDateDisplay { get; init; }
    public required string SectorName { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string OperatorRegistration { get; init; }
    public int Order { get; init; }
    public required string Description { get; init; }
    public string? Instruction { get; init; }
    public string? Observation { get; init; }
    public string? ImageBase64 { get; init; }
    public string? ImageFileName { get; init; }
    public string? ResponsibleName { get; init; }
    public string? ResponsibleSectorName { get; init; }
    public string? ApprovedByName { get; init; }
    public string? ApprovedAtDisplay { get; init; }
    public string? ConcludedByName { get; init; }
    public string? ConcludedAtDisplay { get; init; }
    public string? ResponsibleObservation { get; init; }
    public string? AssignmentObservation { get; init; }
    public string? PlannedCompletionDateDisplay { get; init; }
    public int CompletionPercent { get; init; }
    public required string BackStatus { get; init; }
    public bool CanAssign { get; init; }
    public bool CanUpdate { get; init; }
    public bool CanComplete { get; init; }
    public NonOkWorkflowFormViewModel Form { get; init; } = new();
    public IReadOnlyList<NonOkResponsibleOptionViewModel> ResponsibleOptions { get; init; } = [];
    public IReadOnlyList<NonOkHistoryEntryViewModel> History { get; init; } = [];
}

