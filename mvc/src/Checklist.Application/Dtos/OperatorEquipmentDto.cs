namespace Checklist.Application.Dtos;

public class OperatorEquipmentDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid QrId { get; init; }
    public required string Code { get; init; }
    public required string Description { get; init; }
    public required string CategoryName { get; init; }
    public bool IsActive { get; init; }
}
