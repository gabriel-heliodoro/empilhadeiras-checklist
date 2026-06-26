using System.ComponentModel.DataAnnotations;

namespace Checklist.Mvc.ViewModels;

public class StpDashboardViewModel
{
    public int AreaCount { get; set; }
    public int ChecklistCount { get; set; }
    public int CompanyCount { get; set; }
    public int EmployeeDocumentCount { get; set; }
    public List<StpChecklistListItemViewModel> RecentChecklists { get; set; } = [];
}

public class StpAreaManagementPageViewModel
{
    public List<StpAreaItemViewModel> Items { get; set; } = [];
    public StpAreaFormViewModel Form { get; set; } = new();
    public List<ManagementOptionViewModel> ResponsibleOptions { get; set; } = [];
}

public class StpAreaItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ResponsibleSupervisorId { get; set; }
    public string ResponsibleSupervisorName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class StpAreaFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome da area.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o supervisor responsavel.")]
    [Display(Name = "Responsavel")]
    public Guid ResponsibleSupervisorId { get; set; }

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;
}

public class StpChecklistListPageViewModel
{
    public StpChecklistListFiltersViewModel Filters { get; set; } = new();
    public List<StpChecklistListItemViewModel> Items { get; set; } = [];
}

public class StpChecklistListFiltersViewModel
{
    [Display(Name = "Data inicial")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Data final")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Responsavel")]
    public string? Responsible { get; set; }
}

public class StpChecklistListItemViewModel
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string InspectionAreaName { get; set; } = string.Empty;
    public string ResponsibleName { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int TotalOk { get; set; }
    public int TotalNotOk { get; set; }
    public int TotalNotApplicable { get; set; }
}

public class StpChecklistDetailsViewModel
{
    public Guid Id { get; set; }
    public string InspectionAreaName { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public DateTime ReferenceDate { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string ResponsibleName { get; set; } = string.Empty;

    public string? OtherDeviations { get; set; }
    public string? ObservedPreventiveBehaviors { get; set; }
    public string? ObservedUnsafeActs { get; set; }
    public string? VerifiedUnsafeConditions { get; set; }
    public List<StpChecklistItemViewModel> Items { get; set; } = [];
}

public class StpChecklistItemViewModel
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Instruction { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class StpChecklistEditorPageViewModel
{
    public Guid? AreaId { get; set; }
    public Guid? TemplateId { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public string? ResponsibleName { get; set; }
    public string? TemplateCode { get; set; }
    public string? TemplateName { get; set; }

    [Display(Name = "Outros desvios")]
    public string? OtherDeviations { get; set; }

    [Display(Name = "Comportamentos preventivos observados")]
    public string? ObservedPreventiveBehaviors { get; set; }

    [Display(Name = "Atos inseguros observados")]
    public string? ObservedUnsafeActs { get; set; }

    [Display(Name = "Condicoes inseguras constatadas")]
    public string? VerifiedUnsafeConditions { get; set; }

    public List<ManagementOptionViewModel> AreaOptions { get; set; } = [];
    public List<ManagementOptionViewModel> TemplateOptions { get; set; } = [];
    public List<StpChecklistEditorItemViewModel> Items { get; set; } = [];
}

public class StpChecklistEditorItemViewModel
{
    public Guid TemplateItemId { get; set; }
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Instruction { get; set; }

    [Required(ErrorMessage = "Selecione o resultado do item.")]
    public string Result { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
