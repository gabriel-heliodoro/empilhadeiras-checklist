using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcChecklistItemActionHistory
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ChecklistItemActionId { get; set; }

    public MvcChecklistItemAction ChecklistItemAction { get; set; } = null!;

    [Required]
    public Guid CreatedBySupervisorId { get; set; }

    public MvcSupervisorUser CreatedBySupervisor { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
