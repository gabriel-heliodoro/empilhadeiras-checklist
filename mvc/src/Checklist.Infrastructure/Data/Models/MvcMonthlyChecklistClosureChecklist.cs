using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcMonthlyChecklistClosureChecklist
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MonthlyChecklistClosureId { get; set; }

    public MvcMonthlyChecklistClosure MonthlyChecklistClosure { get; set; } = null!;

    [Required]
    public Guid ChecklistId { get; set; }

    public MvcChecklist Checklist { get; set; } = null!;
}
