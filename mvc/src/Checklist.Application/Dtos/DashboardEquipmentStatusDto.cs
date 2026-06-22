namespace Checklist.Application.Dtos;

public class DashboardEquipmentStatusDto
{
    public Guid EquipmentId { get; init; }
    public required string EquipmentCode { get; init; }
    public required string EquipmentDescription { get; init; }
    public string? CategoryName { get; init; }
    public required string Status { get; init; }
    public Guid? ChecklistId { get; init; }
    public DateTime? ChecklistCompletedAtUtc { get; init; }
}
