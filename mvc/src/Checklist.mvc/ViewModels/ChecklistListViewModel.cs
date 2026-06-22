namespace Checklist.Mvc.ViewModels;

public class ChecklistListViewModel
{
    public ChecklistListFiltersViewModel Filters { get; init; } = new();
    public IReadOnlyList<ChecklistListItemViewModel> Items { get; init; } = [];
}
