using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SafetyWorkReady")]
public class StpController : Controller
{
    private readonly IStpInspectionService _inspectionService;

    public StpController(IStpInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    [HttpGet("stp/dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetDashboardAsync(cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return View(new StpDashboardViewModel());
        }

        return View(new StpDashboardViewModel
        {
            AreaCount = result.Value.AreaCount,
            ChecklistCount = result.Value.ChecklistCount,
            CompanyCount = result.Value.CompanyCount,
            EmployeeDocumentCount = result.Value.EmployeeDocumentCount,
            RecentChecklists = result.Value.RecentChecklists.Select(MapChecklistListItem).ToList()
        });
    }

    [HttpGet("stp/areas")]
    public async Task<IActionResult> Areas([FromQuery] Guid? editId, CancellationToken cancellationToken)
    {
        return View(await BuildAreaPageViewModelAsync(editId, null, cancellationToken));
    }

    [HttpPost("stp/areas")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveArea(StpAreaFormViewModel form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Areas", await BuildAreaPageViewModelAsync(form.Id, form, cancellationToken));
        }

        var result = await _inspectionService.SaveAreaAsync(new StpAreaUpsertDto
        {
            Id = form.Id,
            Name = form.Name,
            ResponsibleSupervisorId = form.ResponsibleSupervisorId,
            IsActive = form.IsActive
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel salvar a area STP.");
            return View("Areas", await BuildAreaPageViewModelAsync(form.Id, form, cancellationToken));
        }

        TempData["StatusMessage"] = form.Id.HasValue ? "Area atualizada." : "Area criada.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Areas), new { editId = form.Id });
    }

    [HttpGet("stp/checklists/new")]
    public async Task<IActionResult> NewChecklist([FromQuery] Guid? areaId, CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetChecklistDraftAsync(areaId, null, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            TempData["StatusMessage"] = result.Error ?? "Nao foi possivel abrir o rascunho da inspecao STP.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Dashboard));
        }

        return View(MapChecklistEditor(result.Value));
    }

    [HttpPost("stp/checklists/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewChecklist(StpChecklistEditorPageViewModel model, CancellationToken cancellationToken)
    {
        if (!model.AreaId.HasValue)
        {
            ModelState.AddModelError(nameof(model.AreaId), "Selecione uma area.");
        }

        if (!model.TemplateId.HasValue)
        {
            ModelState.AddModelError(nameof(model.TemplateId), "Selecione um template.");
        }

        var itemMissingResult = model.Items.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Result));
        if (itemMissingResult is not null)
        {
            ModelState.AddModelError(string.Empty, $"O item {itemMissingResult.Order} precisa de um resultado.");
        }

        if (!ModelState.IsValid)
        {
            return View(await MergeChecklistEditorAsync(model, cancellationToken));
        }

        var result = await _inspectionService.SubmitChecklistAsync(new StpChecklistSubmissionDto
        {
            AreaId = model.AreaId!.Value,
            TemplateId = model.TemplateId!.Value,
            OtherDeviations = model.OtherDeviations,
            ObservedPreventiveBehaviors = model.ObservedPreventiveBehaviors,
            ObservedUnsafeActs = model.ObservedUnsafeActs,
            VerifiedUnsafeConditions = model.VerifiedUnsafeConditions,
            Items = model.Items.Select(x => new StpChecklistSubmissionItemDto
            {
                TemplateItemId = x.TemplateItemId,
                Result = x.Result,
                Notes = x.Notes
            }).ToList()
        }, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel salvar a inspecao STP.");
            return View(await MergeChecklistEditorAsync(model, cancellationToken));
        }

        TempData["StatusMessage"] = $"Inspecao {result.Value.TemplateCode} registrada com sucesso.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    [HttpGet("stp/checklists")]
    public async Task<IActionResult> Checklists([FromQuery] StpChecklistListFiltersViewModel filters, CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetChecklistsAsync(new StpChecklistListFiltersDto
        {
            StartDate = filters.StartDate,
            EndDate = filters.EndDate,
            Responsible = filters.Responsible
        }, cancellationToken);

        if (!result.Success || result.Value is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel carregar os checklists STP.");
            return View(new StpChecklistListPageViewModel { Filters = filters });
        }

        return View(new StpChecklistListPageViewModel
        {
            Filters = filters,
            Items = result.Value.Select(MapChecklistListItem).ToList()
        });
    }

    [HttpGet("stp/checklists/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _inspectionService.GetChecklistDetailsAsync(id, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound();
        }

        return View(new StpChecklistDetailsViewModel
        {
            Id = result.Value.Id,
            InspectionAreaName = result.Value.InspectionAreaName,
            TemplateCode = result.Value.TemplateCode,
            TemplateName = result.Value.TemplateName,
            CompletedAt = result.Value.CompletedAt,
            ReferenceDate = result.Value.ReferenceDate,
            InspectorName = result.Value.InspectorName,
            ResponsibleName = result.Value.ResponsibleName,
            OtherDeviations = result.Value.OtherDeviations,
            ObservedPreventiveBehaviors = result.Value.ObservedPreventiveBehaviors,
            ObservedUnsafeActs = result.Value.ObservedUnsafeActs,
            VerifiedUnsafeConditions = result.Value.VerifiedUnsafeConditions,
            Items = result.Value.Items.Select(x => new StpChecklistItemViewModel
            {
                Order = x.Order,
                Description = x.Description,
                Instruction = x.Instruction,
                Result = x.Result,
                Notes = x.Notes
            }).ToList()
        });
    }

    private async Task<StpAreaManagementPageViewModel> BuildAreaPageViewModelAsync(
        Guid? editId,
        StpAreaFormViewModel? currentForm,
        CancellationToken cancellationToken)
    {
        var areasResult = await _inspectionService.GetAreasAsync(cancellationToken);
        var responsiblesResult = await _inspectionService.GetResponsibleOptionsAsync(cancellationToken);

        var areaItems = areasResult.Success && areasResult.Value is not null
            ? areasResult.Value
            : [];

        var responsibleItems = responsiblesResult.Success && responsiblesResult.Value is not null
            ? responsiblesResult.Value
            : [];

        var form = currentForm ?? new StpAreaFormViewModel();
        if (editId.HasValue && currentForm is null)
        {
            var currentArea = areaItems.FirstOrDefault(x => x.Id == editId.Value);
            if (currentArea is not null)
            {
                form = new StpAreaFormViewModel
                {
                    Id = currentArea.Id,
                    Name = currentArea.Name,
                    ResponsibleSupervisorId = currentArea.ResponsibleSupervisorId,
                    IsActive = currentArea.IsActive
                };
            }
        }

        return new StpAreaManagementPageViewModel
        {
            Form = form,
            ResponsibleOptions = responsibleItems.Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = x.DisplayName
            }).ToList(),
            Items = areaItems.Select(x => new StpAreaItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ResponsibleSupervisorId = x.ResponsibleSupervisorId,
                ResponsibleSupervisorName = x.ResponsibleSupervisorName,
                IsActive = x.IsActive
            }).ToList()
        };
    }

    private async Task<StpChecklistEditorPageViewModel> MergeChecklistEditorAsync(
        StpChecklistEditorPageViewModel model,
        CancellationToken cancellationToken)
    {
        var draftResult = await _inspectionService.GetChecklistDraftAsync(model.AreaId, model.TemplateId, cancellationToken);
        if (!draftResult.Success || draftResult.Value is null)
        {
            return model;
        }

        var draft = MapChecklistEditor(draftResult.Value);
        var postedItems = model.Items.ToDictionary(x => x.TemplateItemId);

        draft.ObservedPreventiveBehaviors = model.ObservedPreventiveBehaviors;
        draft.ObservedUnsafeActs = model.ObservedUnsafeActs;
        draft.VerifiedUnsafeConditions = model.VerifiedUnsafeConditions;
        draft.OtherDeviations = model.OtherDeviations;
        draft.Items = draft.Items.Select(item =>
        {
            if (postedItems.TryGetValue(item.TemplateItemId, out var posted))
            {
                item.Result = posted.Result;
                item.Notes = posted.Notes;
            }

            return item;
        }).ToList();

        return draft;
    }

    private static StpChecklistEditorPageViewModel MapChecklistEditor(StpChecklistDraftDto draft)
    {
        return new StpChecklistEditorPageViewModel
        {
            AreaId = draft.SelectedAreaId,
            TemplateId = draft.SelectedTemplateId,
            InspectorName = draft.InspectorName,
            ResponsibleName = draft.SelectedArea?.ResponsibleSupervisorName,
            TemplateCode = draft.SelectedTemplate?.Code,
            TemplateName = draft.SelectedTemplate?.Name,
            AreaOptions = draft.Areas.Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = x.Name
            }).ToList(),
            TemplateOptions = draft.Templates.Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Name}"
            }).ToList(),
            Items = draft.SelectedTemplate?.Items.Select(x => new StpChecklistEditorItemViewModel
            {
                TemplateItemId = x.Id,
                Order = x.Order,
                Description = x.Description,
                Instruction = x.Instruction,
                Result = string.Empty
            }).ToList() ?? []
        };
    }

    private static StpChecklistListItemViewModel MapChecklistListItem(StpChecklistListItemDto item)
    {
        return new StpChecklistListItemViewModel
        {
            Id = item.Id,
            TemplateId = item.TemplateId,
            TemplateCode = item.TemplateCode,
            TemplateName = item.TemplateName,
            CompletedAt = item.CompletedAt,
            InspectorName = item.InspectorName,
            InspectionAreaName = item.InspectionAreaName,
            ResponsibleName = item.ResponsibleName,
            TotalItems = item.TotalItems,
            TotalOk = item.TotalOk,
            TotalNotOk = item.TotalNotOk,
            TotalNotApplicable = item.TotalNotApplicable
        };
    }
}
