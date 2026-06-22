namespace Checklist.Application.Dtos;

public class SupervisorSessionDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public required string Login { get; init; }
    public required string DisplayName { get; init; }
    public bool ForceChangePassword { get; init; }
    public bool IsMaster { get; init; }
    public required string UserType { get; init; }
    public IReadOnlyCollection<string> ModuleCodes { get; init; } = [];
}
