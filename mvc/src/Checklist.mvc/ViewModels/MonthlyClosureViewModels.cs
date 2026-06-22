using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class MonthlyClosurePageViewModel
{
    public MonthlyClosureFilterViewModel Filters { get; set; } = new();
    public List<ManagementOptionViewModel> EquipmentOptions { get; set; } = [];
    public List<MonthlyClosureSummaryItemViewModel> Closures { get; set; } = [];
    public MonthlyClosurePreviewViewModel? Preview { get; set; }
    public string? PreviewError { get; set; }
}

public class MonthlyClosureFilterViewModel
{
    [Display(Name = "Equipamento")]
    public Guid? EquipmentId { get; set; }

    [Range(2000, 3000)]
    [Display(Name = "Ano")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 12)]
    [Display(Name = "Mes")]
    public int Month { get; set; } = DateTime.Today.Month;
}

public class MonthlyClosureSummaryItemViewModel
{
    public Guid Id { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentDescription { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int ChecklistCount { get; set; }
    public string TemplateVersion { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime ClosedAt { get; set; }
    public string ClosedByName { get; set; } = string.Empty;
}

public class MonthlyClosurePreviewViewModel
{
    public bool IsAlreadyClosed { get; set; }
    public Guid? ClosureId { get; set; }
    public Guid EquipmentId { get; set; }
    public string EquipmentCode { get; set; } = string.Empty;
    public string EquipmentDescription { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDaysWithChecklist { get; set; }
    public int TotalChecklistsConsidered { get; set; }
    public List<MonthlyClosureDayViewModel> Days { get; set; } = [];
    public List<MonthlyClosureRowViewModel> Rows { get; set; } = [];
    public List<string> Comments { get; set; } = [];
    public List<string> ConsolidatedOperators { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class MonthlyClosureDayViewModel
{
    public int Day { get; set; }
    public Guid ChecklistId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string OperatorRegistration { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class MonthlyClosureRowViewModel
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string?> ValuesByDay { get; set; } = [];
}
