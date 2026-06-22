namespace Checklist.Application.Dtos;

public class StpDashboardDto
{
    public int AreaCount { get; init; }
    public int ChecklistCount { get; init; }
    public int CompanyCount { get; init; }
    public int EmployeeDocumentCount { get; init; }
    public IReadOnlyList<StpChecklistListItemDto> RecentChecklists { get; init; } = [];
}

public class StpResponsibleOptionDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}

public class StpAreaSummaryDto
{
    public Guid Id { get; init; }
    public Guid ResponsibleSupervisorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ResponsibleSupervisorName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class StpAreaUpsertDto
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ResponsibleSupervisorId { get; init; }
    public bool IsActive { get; init; } = true;
}

public class StpTemplateSummaryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int ItemCount { get; init; }
}

public class StpTemplateDetailDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<StpTemplateItemDto> Items { get; init; } = [];
}

public class StpTemplateItemDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Instruction { get; init; }
}

public class StpChecklistDraftDto
{
    public Guid? SelectedAreaId { get; init; }
    public Guid? SelectedTemplateId { get; init; }
    public string InspectorName { get; init; } = string.Empty;
    public IReadOnlyList<StpAreaSummaryDto> Areas { get; init; } = [];
    public IReadOnlyList<StpTemplateSummaryDto> Templates { get; init; } = [];
    public StpAreaSummaryDto? SelectedArea { get; init; }
    public StpTemplateDetailDto? SelectedTemplate { get; init; }
}

public class StpChecklistSubmissionDto
{
    public Guid AreaId { get; init; }
    public Guid TemplateId { get; init; }
    public string? ObservedPreventiveBehaviors { get; init; }
    public string? ObservedUnsafeActs { get; init; }
    public string? VerifiedUnsafeConditions { get; init; }
    public IReadOnlyList<StpChecklistSubmissionItemDto> Items { get; init; } = [];
}

public class StpChecklistSubmissionItemDto
{
    public Guid TemplateItemId { get; init; }
    public string Result { get; init; } = string.Empty;
    public string? Notes { get; init; }
}

public class StpChecklistResultDto
{
    public Guid Id { get; init; }
    public DateTime CompletedAt { get; init; }
    public string InspectionAreaName { get; init; } = string.Empty;
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
}

public class StpChecklistListFiltersDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Responsible { get; init; }
}

public class StpChecklistListItemDto
{
    public Guid Id { get; init; }
    public Guid TemplateId { get; init; }
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
    public string InspectorName { get; init; } = string.Empty;
    public string InspectionAreaName { get; init; } = string.Empty;
    public string ResponsibleName { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public int TotalCheck { get; init; }
    public int TotalX { get; init; }
}

public class StpChecklistDetailsDto
{
    public Guid Id { get; init; }
    public string InspectionAreaName { get; init; } = string.Empty;
    public string TemplateCode { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
    public DateTime ReferenceDate { get; init; }
    public string InspectorName { get; init; } = string.Empty;
    public string ResponsibleName { get; init; } = string.Empty;
    public string? ObservedPreventiveBehaviors { get; init; }
    public string? ObservedUnsafeActs { get; init; }
    public string? VerifiedUnsafeConditions { get; init; }
    public IReadOnlyList<StpChecklistItemDto> Items { get; init; } = [];
}

public class StpChecklistItemDto
{
    public int Order { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Instruction { get; init; }
    public string Result { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
