namespace Checklist.Application.Dtos;

public class NonOkPanelItemDto
{
    public Guid ChecklistId { get; init; }
    public Guid ChecklistItemId { get; init; }
    public DateTime ChecklistCompletedAt { get; init; }
    public Guid SourceSectorId { get; init; }
    public required string SourceSectorName { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string OperatorRegistration { get; init; }
    public int Order { get; init; }
    public required string Description { get; init; }
    public string? Instruction { get; init; }
    public string? Notes { get; init; }
    public string? NokImageBase64 { get; init; }
    public string? NokImageFileName { get; init; }
    public string? NokImageMimeType { get; init; }
    public required string WorkflowStatus { get; init; }
    public Guid? ResponsibleSupervisorId { get; init; }
    public string? ResponsibleFullName { get; init; }
    public Guid? ResponsibleSectorId { get; init; }
    public string? ResponsibleSectorName { get; init; }
    public string? AssignmentNotes { get; init; }
    public string? ResponsibleNotes { get; init; }
    public DateTime? PlannedCompletionDate { get; init; }
    public int CompletionPercentage { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? ApprovedByFullName { get; init; }
    public DateTime? WorkflowCompletedAt { get; init; }
    public string? CompletedByFullName { get; init; }
    public IReadOnlyList<NonOkHistoryEntryDto> History { get; init; } = [];
}
