using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Route("operador")]
public class OperatorController : Controller
{
    private readonly IOperatorEquipmentReader _equipmentReader;
    private readonly IOperatorChecklistService _checklistService;
    private readonly ICurrentOperator _currentOperator;

    public OperatorController(
        IOperatorEquipmentReader equipmentReader,
        IOperatorChecklistService checklistService,
        ICurrentOperator currentOperator)
    {
        _equipmentReader = equipmentReader;
        _checklistService = checklistService;
        _currentOperator = currentOperator;
    }

    [Authorize(Policy = "OperatorChecklistReady")]
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? q, CancellationToken cancellationToken)
    {
        var results = Array.Empty<OperatorEquipmentSearchItemViewModel>();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchResult = await _equipmentReader.SearchAsync(q, cancellationToken);
            if (!searchResult.Success)
            {
                ModelState.AddModelError(string.Empty, searchResult.Error ?? "Nao foi possivel buscar os equipamentos.");
            }
            else
            {
                results = searchResult.Value?
                    .Select(MapEquipment)
                    .ToArray() ?? [];
            }
        }

        return View(new OperatorEquipmentSearchViewModel
        {
            Query = q?.Trim() ?? string.Empty,
            OperatorName = _currentOperator.Name ?? string.Empty,
            OperatorRegistration = _currentOperator.Registration ?? string.Empty,
            SectorName = _currentOperator.SectorName ?? string.Empty,
            Results = results
        });
    }

    [Authorize(Policy = "OperatorChecklistReady")]
    [HttpGet("checklists/{equipmentId:guid}")]
    public async Task<IActionResult> Checklist(Guid equipmentId, CancellationToken cancellationToken)
    {
        var result = await _checklistService.GetDraftAsync(equipmentId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            TempData["OperatorErrorMessage"] = result.Error ?? "Nao foi possivel abrir o checklist operacional.";
            return RedirectToAction(nameof(Index));
        }

        return View(MapDraft(result.Value));
    }

    [Authorize(Policy = "OperatorChecklistReady")]
    [HttpGet("checklists/qr/{qrId:guid}")]
    public async Task<IActionResult> ChecklistByQr(Guid qrId, CancellationToken cancellationToken)
    {
        var result = await _equipmentReader.GetByQrIdAsync(qrId, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            TempData["OperatorErrorMessage"] = result.Error ?? "Nao foi possivel localizar o equipamento para este QR ID.";
            return RedirectToAction("Index", "Operation");
        }

        return RedirectToAction(nameof(Checklist), new { equipmentId = result.Value.Id });
    }

    [Authorize(Policy = "OperatorChecklistReady")]
    [HttpPost("checklists/{equipmentId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checklist(Guid equipmentId, OperatorChecklistPageViewModel model, CancellationToken cancellationToken)
    {
        var draftResult = await _checklistService.GetDraftAsync(equipmentId, cancellationToken);
        if (!draftResult.Success || draftResult.Value is null)
        {
            TempData["OperatorErrorMessage"] = draftResult.Error ?? "Nao foi possivel recarregar o checklist.";
            return RedirectToAction(nameof(Index));
        }

        var submission = new OperatorChecklistSubmissionDto
        {
            EquipmentId = equipmentId,
            OperatorId = draftResult.Value.Operator.Id,
            GeneralNotes = model.GeneralNotes,
            OperatorSignatureBase64 = model.SignatureBase64 ?? string.Empty,
            Items = model.Items.Select(item => new OperatorChecklistSubmissionItemDto
            {
                TemplateId = item.TemplateId,
                Status = item.Status,
                Notes = item.Notes,
                NokImageBase64 = item.NokImageBase64,
                NokImageFileName = item.NokImageFileName,
                NokImageMimeType = item.NokImageMimeType
            }).ToList()
        };

        var submitResult = await _checklistService.SubmitAsync(submission, cancellationToken);
        if (!submitResult.Success || submitResult.Value is null)
        {
            ModelState.AddModelError(string.Empty, submitResult.Error ?? "Nao foi possivel enviar o checklist.");
            return View(MergeDraftWithForm(draftResult.Value, model));
        }

        return View("Success", new OperatorChecklistSuccessViewModel
        {
            ChecklistId = submitResult.Value.Id,
            EquipmentCode = submitResult.Value.EquipmentCode,
            OperatorName = submitResult.Value.OperatorName,
            SubmittedAtUtc = submitResult.Value.CompletedAtUtc,
            Status = submitResult.Value.Status
        });
    }

    private OperatorChecklistPageViewModel MapDraft(OperatorChecklistDraftDto draft)
    {
        return new OperatorChecklistPageViewModel
        {
            EquipmentId = draft.Equipment.Id,
            EquipmentCode = draft.Equipment.Code,
            EquipmentDescription = draft.Equipment.Description,
            CategoryName = draft.Equipment.CategoryName,
            EquipmentQrId = draft.Equipment.QrId,
            OperatorName = draft.Operator.Name,
            OperatorRegistration = draft.Operator.Registration,
            SectorName = draft.Operator.SectorName,
            Items = draft.ItemsTemplate
                .Select(item => new OperatorChecklistItemFormViewModel
                {
                    TemplateId = item.Id,
                    Order = item.Order,
                    Description = item.Description,
                    Instruction = item.Instruction,
                    Status = "NotChecked"
                })
                .ToList()
        };
    }

    private OperatorChecklistPageViewModel MergeDraftWithForm(
        OperatorChecklistDraftDto draft,
        OperatorChecklistPageViewModel form)
    {
        var postedItems = form.Items.ToDictionary(item => item.TemplateId);

        return new OperatorChecklistPageViewModel
        {
            EquipmentId = draft.Equipment.Id,
            EquipmentCode = draft.Equipment.Code,
            EquipmentDescription = draft.Equipment.Description,
            CategoryName = draft.Equipment.CategoryName,
            EquipmentQrId = draft.Equipment.QrId,
            OperatorName = draft.Operator.Name,
            OperatorRegistration = draft.Operator.Registration,
            SectorName = draft.Operator.SectorName,
            GeneralNotes = form.GeneralNotes,
            SignatureBase64 = form.SignatureBase64,
            Items = draft.ItemsTemplate
                .Select(template =>
                {
                    if (postedItems.TryGetValue(template.Id, out var postedItem))
                    {
                        postedItem.Order = template.Order;
                        postedItem.Description = template.Description;
                        postedItem.Instruction = template.Instruction;
                        return postedItem;
                    }

                    return new OperatorChecklistItemFormViewModel
                    {
                        TemplateId = template.Id,
                        Order = template.Order,
                        Description = template.Description,
                        Instruction = template.Instruction,
                        Status = "NotChecked"
                    };
                })
                .ToList()
        };
    }

    private static OperatorEquipmentSearchItemViewModel MapEquipment(OperatorEquipmentDto equipment)
    {
        return new OperatorEquipmentSearchItemViewModel
        {
            Id = equipment.Id,
            QrId = equipment.QrId,
            Code = equipment.Code,
            Description = equipment.Description,
            CategoryName = equipment.CategoryName
        };
    }
}
