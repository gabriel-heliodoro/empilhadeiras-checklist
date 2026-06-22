namespace Checklist.Application.Dtos;

public class NonOkResponsibleOptionDto
{
    public Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string Login { get; init; }
    public Guid SectorId { get; init; }
    public required string SectorName { get; init; }
}
