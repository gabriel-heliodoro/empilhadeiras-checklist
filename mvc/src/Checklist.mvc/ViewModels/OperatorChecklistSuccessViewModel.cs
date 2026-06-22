namespace Checklist.Mvc.ViewModels;

public class OperatorChecklistSuccessViewModel
{
    public Guid ChecklistId { get; init; }
    public required string EquipmentCode { get; init; }
    public required string OperatorName { get; init; }
    public DateTime SubmittedAtUtc { get; init; }
    public required string Status { get; init; }
}
