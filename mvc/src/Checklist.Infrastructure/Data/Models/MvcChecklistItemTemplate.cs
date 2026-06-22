using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcChecklistItemTemplate
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid SectorId { get; set; }

    public MvcSector Sector { get; set; } = null!;

    [Required]
    public Guid CategoryId { get; set; }

    public MvcEquipmentCategory Category { get; set; } = null!;

    public int Order { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? Instruction { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
