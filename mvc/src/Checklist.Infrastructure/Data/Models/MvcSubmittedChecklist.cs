using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcSubmittedChecklist
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid EquipmentId { get; set; }

    public MvcEquipment Equipment { get; set; } = null!;

    [Required]
    public string OperatorName { get; set; } = string.Empty;

    [Required]
    public string OperatorRegistration { get; set; } = string.Empty;

    public bool IsApproved { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
