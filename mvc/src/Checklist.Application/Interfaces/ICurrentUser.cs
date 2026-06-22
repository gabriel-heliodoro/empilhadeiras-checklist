namespace Checklist.Application.Interfaces;

public interface ICurrentUser
{
    Guid? Id { get; }
    Guid? SectorId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool ForceChangePassword { get; }
    bool IsMaster { get; }
    string? UserType { get; }
    IReadOnlyCollection<string> ModuleCodes { get; }
}
