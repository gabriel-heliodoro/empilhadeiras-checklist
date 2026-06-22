using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcEquipmentCategory
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid SectorId { get; set; }
    public MvcSector Sector { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;
    public MvcMonthlyClosureModel MonthlyClosureModel { get; set; } = MvcMonthlyClosureModel.None;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<MvcEquipment> Equipments { get; set; } = [];
    public List<MvcChecklistItemTemplate> ChecklistItemTemplates { get; set; } = [];
}
