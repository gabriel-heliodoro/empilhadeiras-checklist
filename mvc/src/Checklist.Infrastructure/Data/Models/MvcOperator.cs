using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcOperator
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SectorId { get; set; }

    public MvcSector Sector { get; set; } = null!;

    [Required]
    [MaxLength(40)]
    public string Registration { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool ForceChangePassword { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
