namespace Checklist.Application.Dtos;

public class ChecklistItemDto
{
    public required string Label { get; init; }
    public required string Status { get; init; }
    public string? Notes { get; init; }
}
