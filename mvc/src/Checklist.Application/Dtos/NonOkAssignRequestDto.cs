namespace Checklist.Application.Dtos;

public class NonOkAssignRequestDto
{
    public Guid ResponsibleSupervisorId { get; init; }
    public string? AssignmentObservation { get; init; }
    public string? ResponsibleObservation { get; init; }
    public DateTime? PlannedCompletionDate { get; init; }
    public int CompletionPercent { get; init; }
}
