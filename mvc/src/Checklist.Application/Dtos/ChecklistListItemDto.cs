namespace Checklist.Application.Dtos;

public class ChecklistListItemDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public required string OperatorName { get; init; }
    public required string OperatorRegistration { get; init; }
    public DateTime CreatedAt { get; init; }
    public required string Status { get; init; }
    public int TotalItems { get; init; }
    public int ItemsOk { get; init; }
    public int ItemsNok { get; init; }
}
