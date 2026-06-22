namespace Checklist.Mvc.ViewModels;

public class NonOkHistoryEntryViewModel
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string CreatedAtDisplay { get; init; }
    public required string CreatedByDisplayName { get; init; }
}
