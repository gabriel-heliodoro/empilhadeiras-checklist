using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcStpAreaChecklist
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SectorId { get; set; }

    public MvcSector Sector { get; set; } = null!;

    [Required]
    public Guid InspectedSectorId { get; set; }

    public MvcSector InspectedSector { get; set; } = null!;

    public Guid? InspectionAreaId { get; set; }
    public MvcStpInspectionArea? InspectionArea { get; set; }

    [Required]
    public Guid TemplateId { get; set; }

    public MvcStpAreaChecklistTemplate Template { get; set; } = null!;

    [Required]
    public Guid InspectorSupervisorId { get; set; }

    public MvcSupervisorUser InspectorSupervisor { get; set; } = null!;

    [Required]
    [MaxLength(160)]
    public string PresentResponsibleName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? PresentResponsibleRole { get; set; }

    [MaxLength(4000)]
    public string? OtherDeviations { get; set; }

    [MaxLength(4000)]
    public string? ObservedPreventiveBehaviors { get; set; }

    [MaxLength(4000)]
    public string? ObservedUnsafeActs { get; set; }

    [MaxLength(4000)]
    public string? VerifiedUnsafeConditions { get; set; }

    [Required]
    public string InspectorSignatureBase64 { get; set; } = string.Empty;

    [Required]
    public string PresentResponsibleSignatureBase64 { get; set; } = string.Empty;

    public DateTime InspectorSignedAt { get; set; } = DateTime.UtcNow;
    public DateTime PresentResponsibleSignedAt { get; set; } = DateTime.UtcNow;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public DateTime ReferenceDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<MvcStpAreaChecklistItem> Items { get; set; } = [];
}
