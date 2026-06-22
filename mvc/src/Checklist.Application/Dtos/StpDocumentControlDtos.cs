namespace Checklist.Application.Dtos;

public class StpCompanySummaryDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int TotalDocuments { get; init; }
    public int TotalEmployees { get; init; }
}

public class StpCompanyUpsertDto
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}

public class StpCompanyDetailsDto
{
    public StpCompanySummaryDto Company { get; init; } = new();
    public IReadOnlyList<StpDocumentFileDto> Documents { get; init; } = [];
    public IReadOnlyList<StpEmployeeSummaryDto> Employees { get; init; } = [];
}

public class StpEmployeeSummaryDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Role { get; init; }
    public bool IsActive { get; init; }
    public int TotalDocuments { get; init; }
}

public class StpEmployeeUpsertDto
{
    public Guid? Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Role { get; init; }
    public bool IsActive { get; init; } = true;
}

public class StpEmployeeDetailsDto
{
    public StpCompanySummaryDto Company { get; init; } = new();
    public StpEmployeeSummaryDto Employee { get; init; } = new();
    public IReadOnlyList<StpDocumentFileDto> Documents { get; init; } = [];
}

public class StpDocumentUploadDto
{
    public string? Name { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = "application/octet-stream";
    public long SizeInBytes { get; init; }
    public byte[] Content { get; init; } = [];
}

public class StpDocumentFileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class StpDocumentFileContentDto
{
    public string FileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = "application/octet-stream";
    public byte[] Content { get; init; } = [];
}
