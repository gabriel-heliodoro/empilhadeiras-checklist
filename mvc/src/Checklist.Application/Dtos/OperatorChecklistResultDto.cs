namespace Checklist.Application.Dtos;

public class OperatorChecklistResultDto
{
    public Guid Id { get; init; }
    public Guid SectorId { get; init; }
    public Guid EquipmentId { get; init; }
    public required string EquipmentCode { get; init; }
    public Guid OperatorId { get; init; }
    public required string OperatorName { get; init; }
    public DateTime CompletedAtUtc { get; init; }
    public bool IsApproved { get; init; }
    public required string Status { get; init; }
}
