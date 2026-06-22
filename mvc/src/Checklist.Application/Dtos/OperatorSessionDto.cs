namespace Checklist.Application.Dtos;

public class OperatorSessionDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public required string Name { get; init; }
    public required string Registration { get; init; }
    public required string Login { get; init; }
    public required string SectorName { get; init; }
    public bool ForceChangePassword { get; init; }
}
