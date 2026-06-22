namespace Checklist.Mvc.ViewModels;

public class NonOkWorkflowFormViewModel
{
    public string BackStatus { get; set; } = "pending";
    public Guid? ResponsibleSupervisorId { get; set; }
    public string? AssignmentObservation { get; set; }
    public string? ResponsibleObservation { get; set; }
    public string? PlannedCompletionDate { get; set; }
    public int CompletionPercent { get; set; }
}
