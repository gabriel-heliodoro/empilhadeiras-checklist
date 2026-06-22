using Checklist.Infrastructure.Data.Models;

namespace Checklist.Infrastructure.Services;

public class ChecklistMonthlySnapshot
{
    public Guid EquipmentId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public MvcMonthlyClosureModel MonthlyClosureModel { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentDescription { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public int TotalDaysWithChecklist { get; set; }
    public int TotalChecklistsConsidered { get; set; }
    public List<ChecklistMonthlyRowSnapshot> Rows { get; set; } = [];
    public List<ChecklistMonthlyDaySnapshot> Days { get; set; } = [];
    public List<string> Comments { get; set; } = [];
    public List<string> ConsolidatedOperators { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class ChecklistMonthlyRowSnapshot
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string?> ValuesByDay { get; set; } = [];
}

public class ChecklistMonthlyDaySnapshot
{
    public int Day { get; set; }
    public Guid ChecklistId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string OperatorRegistration { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
