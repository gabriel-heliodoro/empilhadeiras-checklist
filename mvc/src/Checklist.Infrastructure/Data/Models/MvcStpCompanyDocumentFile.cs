using System.ComponentModel.DataAnnotations;

namespace Checklist.Infrastructure.Data.Models;

public class MvcStpCompanyDocumentFile
{
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CompanyId { get; set; }

    public MvcStpCompanyDocument Company { get; set; } = null!;

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string MimeType { get; set; } = "application/octet-stream";

    public long SizeInBytes { get; set; }

    [Required]
    public byte[] Content { get; set; } = [];

    [Required]
    public Guid UploadedBySupervisorId { get; set; }

    public MvcSupervisorUser UploadedBySupervisor { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
