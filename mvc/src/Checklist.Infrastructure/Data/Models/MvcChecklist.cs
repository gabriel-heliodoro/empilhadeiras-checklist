using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcChecklist
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
    public Guid OperatorId { get; set; }

    public MvcOperator Operator { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public DateTime ReferenceDate { get; set; } = DateTime.UtcNow.Date;
    public bool IsApproved { get; set; }
    public string? GeneralNotes { get; set; }
    public string? OperatorSignatureBase64 { get; set; }
    public DateTime? SignedAt { get; set; }
    public MvcChecklistStatus Status { get; set; } = MvcChecklistStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MvcChecklistItem> Items { get; set; } = [];
}
