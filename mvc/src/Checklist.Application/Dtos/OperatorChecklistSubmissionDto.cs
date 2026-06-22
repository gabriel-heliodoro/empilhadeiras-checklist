namespace Checklist.Application.Dtos;

public class OperatorChecklistSubmissionDto
{
    public Guid EquipmentId { get; init; }
    public Guid OperatorId { get; init; }
    public string? GeneralNotes { get; init; }
    public required string OperatorSignatureBase64 { get; init; }
    public IReadOnlyList<OperatorChecklistSubmissionItemDto> Items { get; init; } = [];
}
