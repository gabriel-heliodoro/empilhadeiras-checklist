namespace Checklist.Application.Dtos;

public class OperatorChecklistDraftDto
{
    public required OperatorEquipmentDto Equipment { get; init; }
    public required OperatorSessionDto Operator { get; init; }
    public IReadOnlyList<OperatorChecklistTemplateItemDto> ItemsTemplate { get; init; } = [];
}
