using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcStpEmployeeDocument
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CompanyId { get; set; }

    public MvcStpCompanyDocument Company { get; set; } = null!;

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Role { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<MvcStpEmployeeDocumentFile> Documents { get; set; } = [];
}
