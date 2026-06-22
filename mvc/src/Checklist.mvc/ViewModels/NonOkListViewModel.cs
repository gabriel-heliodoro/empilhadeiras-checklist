namespace Checklist.Mvc.ViewModels;

public class NonOkListViewModel
{
    public NonOkFiltersViewModel Filters { get; init; } = new();
    public required string ActiveStatus { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public int TotalPanelCount { get; init; }
    public int ActiveCount { get; init; }
    public int PendingCount { get; init; }
    public int InProgressCount { get; init; }
    public int CompletedCount { get; init; }
    public IReadOnlyList<NonOkItemViewModel> Items { get; init; } = [];
}
