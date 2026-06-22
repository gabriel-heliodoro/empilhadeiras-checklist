using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcSupervisorUserModule
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SupervisorUserId { get; set; }
    public MvcSupervisorUser SupervisorUser { get; set; } = null!;
    public MvcAccessModule Module { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
