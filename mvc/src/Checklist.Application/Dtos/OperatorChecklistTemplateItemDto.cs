namespace Checklist.Application.Dtos;

public class OperatorChecklistTemplateItemDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public Guid CategoryId { get; init; }
    public int Order { get; init; }
    public required string Description { get; init; }
    public string? Instruction { get; init; }
    public bool IsActive { get; init; }
}
