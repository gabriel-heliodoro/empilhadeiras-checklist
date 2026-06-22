using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SectorSupervisorReady")]
public class NonOkController : Controller
{
    private readonly INonOkReader _nonOkReader;
    private readonly INonOkWorkflowService _nonOkWorkflowService;

    public NonOkController(INonOkReader nonOkReader, INonOkWorkflowService nonOkWorkflowService)
    {
        _nonOkReader = nonOkReader;
        _nonOkWorkflowService = nonOkWorkflowService;
    }

    [HttpGet("non-ok")]
    public async Task<IActionResult> Index([FromQuery] NonOkFiltersViewModel filters, CancellationToken cancellationToken)
    {
        var result = await _nonOkReader.GetPanelAsync(MapFilters(filters), cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error ?? "Falha ao carregar o painel de itens non-compliant.");
        }

        var model = new NonOkDashboardViewModel
        {
            PendingCount = result.Value.PendingApproval.Count,
            InProgressCount = result.Value.InProgress.Count,
            CompletedCount = result.Value.Completed.Count
        };

        return View(model);
    }

    [HttpGet("non-ok/lista")]
    public async Task<IActionResult> Lista([FromQuery] string? status, [FromQuery] NonOkFiltersViewModel filters, CancellationToken cancellationToken)
    {
        var result = await _nonOkReader.GetPanelAsync(MapFilters(filters), cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error ?? "Falha ao carregar a lista de itens non-compliant.");
        }

        var activeStatus = NormalizeStatus(status);
        var (title, description, items) = activeStatus switch
        {
            "in-progress" => ("Em andamento", "Tratativas aprovadas que ainda exigem acao.", result.Value.InProgress),
            "completed" => ("Completed", "Items encerrados para consulta e historico.", result.Value.Completed),
            _ => ("Pending", "Items aguardando aprovacao e atribuicao.", result.Value.PendingApproval)
        };

        var model = new NonOkListViewModel
        {
            Filters = filters,
            ActiveStatus = activeStatus,
            Title = title,
            Description = description,
            TotalPanelCount = result.Value.PendingApproval.Count + result.Value.InProgress.Count + result.Value.Completed.Count,
            ActiveCount = items.Count,
            PendingCount = result.Value.PendingApproval.Count,
            InProgressCount = result.Value.InProgress.Count,
            CompletedCount = result.Value.Completed.Count,
            Items = items.Select(MapItem).ToList()
        };

        return View(model);
    }

    [HttpGet("non-ok/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _nonOkReader.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound(result.Error ?? "Item non-compliant nao encontrado.");
        }

        var responsibleOptions = await LoadResponsibleOptionsAsync(cancellationToken);
        return View(MapDetails(result.Value, NormalizeStatus(status), responsibleOptions, null));
    }

    [HttpPost("non-ok/{id:guid}/atribuir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid id, NonOkWorkflowFormViewModel form, CancellationToken cancellationToken)
    {
        if (!form.ResponsibleSupervisorId.HasValue)
        {
            TempData["NonOkErrorMessage"] = "Selecione um responsavel antes de atribuir a tratativa.";
            return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
        }

        var result = await _nonOkWorkflowService.AssignAsync(id, new NonOkAssignRequestDto
        {
            ResponsibleSupervisorId = form.ResponsibleSupervisorId.Value,
            AssignmentObservation = form.AssignmentObservation,
            ResponsibleObservation = form.ResponsibleObservation,
            PlannedCompletionDate = ParseDateOnly(form.PlannedCompletionDate),
            CompletionPercent = form.CompletionPercent
        }, cancellationToken);

        if (!result.Success)
        {
            TempData["NonOkErrorMessage"] = result.Error ?? "Falha ao atribuir a tratativa.";
            return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
        }

        TempData["NonOkSuccessMessage"] = "Tratativa atribuida com sucesso.";
        return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
    }

    [HttpPost("non-ok/{id:guid}/tratativa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, NonOkWorkflowFormViewModel form, CancellationToken cancellationToken)
    {
        if (!form.ResponsibleSupervisorId.HasValue)
        {
            TempData["NonOkErrorMessage"] = "Selecione um responsavel antes de salvar a tratativa.";
            return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
        }

        var result = await _nonOkWorkflowService.UpdateAsync(id, new NonOkUpdateRequestDto
        {
            ResponsibleSupervisorId = form.ResponsibleSupervisorId.Value,
            ResponsibleObservation = form.ResponsibleObservation,
            PlannedCompletionDate = ParseDateOnly(form.PlannedCompletionDate),
            CompletionPercent = form.CompletionPercent
        }, cancellationToken);

        if (!result.Success)
        {
            TempData["NonOkErrorMessage"] = result.Error ?? "Falha ao atualizar a tratativa.";
            return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
        }

        TempData["NonOkSuccessMessage"] = "Tratativa atualizada com sucesso.";
        return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(form.BackStatus) });
    }

    [HttpPost("non-ok/{id:guid}/concluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, string backStatus, CancellationToken cancellationToken)
    {
        var result = await _nonOkWorkflowService.CompleteAsync(id, cancellationToken);
        if (!result.Success)
        {
            TempData["NonOkErrorMessage"] = result.Error ?? "Falha ao concluir a tratativa.";
            return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(backStatus) });
        }

        TempData["NonOkSuccessMessage"] = "Tratativa concluida com sucesso.";
        return RedirectToAction(nameof(Details), new { id, status = NormalizeStatus(backStatus) });
    }

    private static NonOkFiltersDto MapFilters(NonOkFiltersViewModel filters)
    {
        return new NonOkFiltersDto
        {
            DataInicio = filters.DataInicio,
            DataFim = filters.DataFim,
            Equipment = filters.Equipment,
            Operator = filters.Operator
        };
    }

    private static string NormalizeStatus(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "andamento" or "in-progress" => "in-progress",
            "concluidas" or "completed" => "completed",
            _ => "pending"
        };
    }

    private static NonOkItemViewModel MapItem(NonOkPanelItemDto item)
    {
        var (label, cssClass) = item.WorkflowStatus switch
        {
            "in-progress" => ("Em andamento", "text-bg-warning"),
            "completed" => ("Completed", "text-bg-success"),
            _ => ("Pending", "text-bg-danger")
        };

        return new NonOkItemViewModel
        {
            ChecklistId = item.ChecklistId,
            ChecklistItemId = item.ChecklistItemId,
            EquipmentCode = item.EquipmentCode,
            EquipmentDescription = item.EquipmentDescription,
            OperatorName = item.OperatorName,
            OperatorRegistration = item.OperatorRegistration,
            SectorName = item.SourceSectorName,
            Order = item.Order,
            Description = item.Description,
            Instruction = item.Instruction,
            Observation = item.Notes,
            WorkflowStatus = item.WorkflowStatus,
            WorkflowLabel = label,
            WorkflowCssClass = cssClass,
            ResponsibleName = item.ResponsibleFullName,
            ResponsibleSectorName = item.ResponsibleSectorName,
            CompletionPercent = item.CompletionPercentage,
            PlannedCompletionDateDisplay = item.PlannedCompletionDate?.ToString("dd/MM/yyyy"),
            ChecklistDateDisplay = item.ChecklistCompletedAt.ToString("dd/MM/yyyy HH:mm")
        };
    }

    private static NonOkDetailsViewModel MapDetails(
        NonOkPanelItemDto item,
        string backStatus,
        IReadOnlyList<NonOkResponsibleOptionViewModel> responsibleOptions,
        NonOkWorkflowFormViewModel? form)
    {
        var (label, cssClass) = item.WorkflowStatus switch
        {
            "in-progress" => ("Em andamento", "text-bg-warning"),
            "completed" => ("Completed", "text-bg-success"),
            _ => ("Pending", "text-bg-danger")
        };

        var resolvedForm = form ?? new NonOkWorkflowFormViewModel
        {
            BackStatus = backStatus,
            ResponsibleSupervisorId = item.ResponsibleSupervisorId,
            AssignmentObservation = item.AssignmentNotes,
            ResponsibleObservation = item.ResponsibleNotes,
            PlannedCompletionDate = item.PlannedCompletionDate?.ToString("yyyy-MM-dd"),
            CompletionPercent = item.CompletionPercentage
        };

        return new NonOkDetailsViewModel
        {
            ChecklistItemId = item.ChecklistItemId,
            ChecklistId = item.ChecklistId,
            WorkflowStatus = item.WorkflowStatus,
            WorkflowLabel = label,
            WorkflowCssClass = cssClass,
            ChecklistDateDisplay = item.ChecklistCompletedAt.ToString("dd/MM/yyyy HH:mm"),
            SectorName = item.SourceSectorName,
            EquipmentCode = item.EquipmentCode,
            EquipmentDescription = item.EquipmentDescription,
            OperatorName = item.OperatorName,
            OperatorRegistration = item.OperatorRegistration,
            Order = item.Order,
            Description = item.Description,
            Instruction = item.Instruction,
            Observation = item.Notes,
            ImageBase64 = item.NokImageBase64,
            ImageFileName = item.NokImageFileName,
            ResponsibleName = item.ResponsibleFullName,
            ResponsibleSectorName = item.ResponsibleSectorName,
            ApprovedByName = item.ApprovedByFullName,
            ApprovedAtDisplay = item.ApprovedAt?.ToString("dd/MM/yyyy HH:mm"),
            ConcludedByName = item.CompletedByFullName,
            ConcludedAtDisplay = item.WorkflowCompletedAt?.ToString("dd/MM/yyyy HH:mm"),
            ResponsibleObservation = item.ResponsibleNotes,
            AssignmentObservation = item.AssignmentNotes,
            PlannedCompletionDateDisplay = item.PlannedCompletionDate?.ToString("dd/MM/yyyy"),
            CompletionPercent = item.CompletionPercentage,
            BackStatus = backStatus,
            CanAssign = item.WorkflowStatus == "pending-approval",
            CanUpdate = item.WorkflowStatus == "in-progress",
            CanComplete = item.WorkflowStatus != "completed",
            Form = resolvedForm,
            ResponsibleOptions = responsibleOptions,
            History = item.History
                .OrderByDescending(entry => entry.CreatedAt)
                .Select(entry => new NonOkHistoryEntryViewModel
                {
                    Title = entry.Title,
                    Description = entry.Description,
                    CreatedAtDisplay = entry.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    CreatedByDisplayName = entry.CreatedByDisplayName
                })
                .ToList()
        };
    }

    private async Task<IReadOnlyList<NonOkResponsibleOptionViewModel>> LoadResponsibleOptionsAsync(CancellationToken cancellationToken)
    {
        var result = await _nonOkWorkflowService.ListResponsibleOptionsAsync(cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return [];
        }

        return result.Value
            .Select(option => new NonOkResponsibleOptionViewModel
            {
                Id = option.Id,
                DisplayName = $"{option.FullName} - {option.SectorName}"
            })
            .ToList();
    }

    private static DateTime? ParseDateOnly(string? value)
    {
        if (!DateTime.TryParse(value, out var parsed))
        {
            return null;
        }

        return new DateTime(parsed.Year, parsed.Month, parsed.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}

