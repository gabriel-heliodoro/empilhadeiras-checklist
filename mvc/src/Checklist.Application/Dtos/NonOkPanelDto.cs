namespace Checklist.Application.Dtos;

public class NonOkPanelDto
{
    public IReadOnlyList<NonOkPanelItemDto> PendingApproval { get; init; } = [];
    public IReadOnlyList<NonOkPanelItemDto> InProgress { get; init; } = [];
    public IReadOnlyList<NonOkPanelItemDto> Completed { get; init; } = [];
}
