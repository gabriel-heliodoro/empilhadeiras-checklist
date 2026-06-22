using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcStpAreaChecklistItem
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ChecklistId { get; set; }

    public MvcStpAreaChecklist Checklist { get; set; } = null!;

    [Required]
    public Guid TemplateItemId { get; set; }

    public MvcStpAreaChecklistTemplateItem TemplateItem { get; set; } = null!;

    public int Order { get; set; }

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Instruction { get; set; }

    public MvcStpAreaChecklistResult Result { get; set; } = MvcStpAreaChecklistResult.Check;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
