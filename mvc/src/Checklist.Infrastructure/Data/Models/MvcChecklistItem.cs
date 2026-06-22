using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcChecklistItem
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid ChecklistId { get; set; }

    public MvcChecklist Checklist { get; set; } = null!;

    [Required]
    public Guid TemplateId { get; set; }

    public MvcChecklistItemTemplate Template { get; set; } = null!;

    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public MvcItemStatus Status { get; set; } = MvcItemStatus.NotChecked;
    public string? Notes { get; set; }
    public string? NokImageBase64 { get; set; }
    public string? NokImageFileName { get; set; }
    public string? NokImageMimeType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MvcChecklistItemAction? Action { get; set; }
}
