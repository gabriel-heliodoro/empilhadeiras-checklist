namespace Checklist.Application.Interfaces;

public interface ICurrentOperator
{
    Guid? Id { get; }
    Guid? SectorId { get; }
    string? SectorName { get; }
    string? Name { get; }
    string? Registration { get; }
    bool IsAuthenticated { get; }
    bool ForceChangePassword { get; }
}
