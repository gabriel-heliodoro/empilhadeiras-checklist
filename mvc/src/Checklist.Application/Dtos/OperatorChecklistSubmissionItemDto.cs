namespace Checklist.Application.Dtos;

public class OperatorChecklistSubmissionItemDto
{
    public Guid TemplateId { get; init; }
    public required string Status { get; init; }
    public string? Notes { get; init; }
    public string? NokImageBase64 { get; init; }
    public string? NokImageFileName { get; init; }
    public string? NokImageMimeType { get; init; }
}
