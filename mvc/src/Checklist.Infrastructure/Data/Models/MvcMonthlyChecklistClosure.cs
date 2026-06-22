using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcMonthlyChecklistClosure
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SectorId { get; set; }

    public MvcSector Sector { get; set; } = null!;

    [Required]
    public Guid EquipmentId { get; set; }

    public MvcEquipment Equipment { get; set; } = null!;

    [Required]
    public Guid ClosedBySupervisorId { get; set; }

    public MvcSupervisorUser ClosedBySupervisor { get; set; } = null!;

    public int Year { get; set; }
    public int Month { get; set; }

    [Required]
    [MaxLength(120)]
    public string TemplateName { get; set; } = "Checklist - Empilhadeiras";

    [Required]
    [MaxLength(40)]
    public string TemplateVersion { get; set; } = "v1";

    public MvcMonthlyChecklistClosureStatus Status { get; set; } = MvcMonthlyChecklistClosureStatus.Closed;

    [Required]
    public string SnapshotJson { get; set; } = string.Empty;

    [Required]
    [MaxLength(180)]
    public string PdfFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string PdfSha256Hash { get; set; } = string.Empty;

    [Required]
    public byte[] PdfContent { get; set; } = [];

    public int ChecklistCount { get; set; }
    public DateTime ClosedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<MvcMonthlyChecklistClosureChecklist> Checklists { get; set; } = [];
}
