namespace Checklist.Mvc.ViewModels;

public class NonOkDashboardViewModel
{
    public int PendingCount { get; init; }
    public int InProgressCount { get; init; }
    public int CompletedCount { get; init; }
    public int TotalCount => PendingCount + InProgressCount + CompletedCount;
}
