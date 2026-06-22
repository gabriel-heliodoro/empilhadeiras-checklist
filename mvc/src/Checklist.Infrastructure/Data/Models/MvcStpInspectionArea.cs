using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcStpInspectionArea
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SectorId { get; set; }

    public MvcSector Sector { get; set; } = null!;

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid ResponsibleSupervisorId { get; set; }

    public MvcSupervisorUser ResponsibleSupervisor { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
