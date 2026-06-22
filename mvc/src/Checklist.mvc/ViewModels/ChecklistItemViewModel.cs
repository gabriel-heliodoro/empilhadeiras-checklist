namespace Checklist.Mvc.ViewModels;

public class ChecklistItemViewModel
{
    public required string Label { get; init; }
    public required string Status { get; init; }
    public string? Notes { get; init; }
}
