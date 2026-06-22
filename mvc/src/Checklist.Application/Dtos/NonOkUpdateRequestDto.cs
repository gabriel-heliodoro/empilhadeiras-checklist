namespace Checklist.Application.Dtos;

public class NonOkUpdateRequestDto
{
    public Guid ResponsibleSupervisorId { get; init; }
    public string? ResponsibleObservation { get; init; }
    public DateTime? PlannedCompletionDate { get; init; }
    public int CompletionPercent { get; init; }
}
