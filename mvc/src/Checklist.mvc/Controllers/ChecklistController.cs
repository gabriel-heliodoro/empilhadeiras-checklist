using Checklist.Application.Interfaces;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SectorSupervisorReady")]
public class ChecklistController : Controller
{
    private readonly IChecklistReader _checklistReader;

    public ChecklistController(IChecklistReader checklistReader)
    {
        _checklistReader = checklistReader;
    }

    [HttpGet("checklists")]
    public async Task<IActionResult> Index([FromQuery] ChecklistListFiltersViewModel filters, CancellationToken cancellationToken)
    {
        var result = await _checklistReader.ListAsync(
            new Checklist.Application.Dtos.ChecklistListFiltersDto
            {
                DataInicio = filters.DataInicio,
                DataFim = filters.DataFim,
                Status = filters.Status,
                Operator = filters.Operator
            },
            cancellationToken);

        if (!result.Success || result.Value is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error ?? "Falha ao carregar a lista de checklists.");
        }

        var model = new ChecklistListViewModel
        {
            Filters = filters,
            Items = result.Value.Select(MapListItem).ToList()
        };

        return View(model);
    }

    [HttpGet("checklists/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _checklistReader.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return NotFound(result.Error ?? "Checklist não encontrado.");
        }

        var model = new ChecklistDetailsViewModel
        {
            Id = result.Value.Id,
            Code = result.Value.Code,
            EquipmentCode = result.Value.EquipmentCode,
            EquipmentDescription = result.Value.EquipmentDescription,
            OperatorName = result.Value.OperatorName,
            SectorName = result.Value.SectorName,
            Status = result.Value.Status,
            CreatedAtUtc = result.Value.CreatedAtUtc,
            Items = result.Value.Items
                .Select(item => new ChecklistItemViewModel
                {
                    Label = item.Label,
                    Status = item.Status,
                    Notes = item.Notes
                })
                .ToList()
        };

        return View(model);
    }

    private static ChecklistListItemViewModel MapListItem(Checklist.Application.Dtos.ChecklistListItemDto item)
    {
        var (label, cssClass) = item.Status switch
        {
            "ok" => ("OK", "text-bg-success"),
            "nok" => ("NOK", "text-bg-danger"),
            _ => ("Indefinido", "text-bg-secondary")
        };

        return new ChecklistListItemViewModel
        {
            Id = item.Id,
            EquipmentCode = item.EquipmentCode,
            EquipmentDescription = item.EquipmentDescription,
            OperatorName = item.OperatorName,
            OperatorRegistration = item.OperatorRegistration,
            CreatedAtDisplay = item.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
            StatusKey = item.Status,
            StatusLabel = label,
            StatusCssClass = cssClass,
            TotalItems = item.TotalItems,
            ItemsOk = item.ItemsOk,
            ItemsNok = item.ItemsNok
        };
    }
}
