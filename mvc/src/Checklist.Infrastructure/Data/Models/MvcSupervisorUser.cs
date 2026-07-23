using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcSupervisorUser
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Login { get; set; } = string.Empty;

    [MaxLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Extension { get; set; }

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool ForceChangePassword { get; set; } = false;
    public bool IsMaster { get; set; } = false;
    public MvcUserAccessType UserType { get; set; } = MvcUserAccessType.Supervisor;

    [Required]
    public Guid SectorId { get; set; }
    public MvcSector Sector { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MvcSupervisorUserModule> Modules { get; set; } = [];
}
