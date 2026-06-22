namespace Checklist.Application.Dtos;

public class NonOkHistoryEntryDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string CreatedByDisplayName { get; init; }
}
