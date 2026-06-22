using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcChecklistItemAction
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ChecklistItemId { get; set; }

    public MvcChecklistItem ChecklistItem { get; set; } = null!;

    public MvcChecklistItemActionStatus Status { get; set; } = MvcChecklistItemActionStatus.InProgress;

    [Required]
    public Guid ApprovedBySupervisorId { get; set; }

    public MvcSupervisorUser ApprovedBySupervisor { get; set; } = null!;
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    public Guid? ResponsibleSupervisorId { get; set; }
    public MvcSupervisorUser? ResponsibleSupervisor { get; set; }
    public Guid? ResponsibleSectorId { get; set; }
    public MvcSector? ResponsibleSector { get; set; }
    public string? AssignmentNotes { get; set; }
    public string? ResponsibleNotes { get; set; }
    public DateTime? PlannedCompletionDate { get; set; }
    public int CompletionPercentage { get; set; }
    public Guid? CompletedBySupervisorId { get; set; }
    public MvcSupervisorUser? CompletedBySupervisor { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MvcChecklistItemActionHistory> History { get; set; } = [];
}
