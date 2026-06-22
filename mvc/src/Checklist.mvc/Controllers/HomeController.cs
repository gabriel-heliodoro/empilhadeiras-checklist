using System.Diagnostics;
using System.Globalization;
using Checklist.Application.Interfaces;
using Checklist.Mvc.Models;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SectorSupervisorReady")]
public class HomeController : Controller
{
    private static readonly StringComparer PtBrComparer = StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: true);
    private readonly IDashboardReader _dashboardReader;

    public HomeController(IDashboardReader dashboardReader)
    {
        _dashboardReader = dashboardReader;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _dashboardReader.GetSummaryAsync(cancellationToken);
        if (!result.Success || result.Value is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error ?? "Falha ao carregar a dashboard.");
        }

        var model = new DashboardViewModel
        {
            UserDisplayName = result.Value.UserDisplayName,
            CurrentUtcTimestamp = result.Value.CurrentUtcTimestamp,
            ChecklistCount = result.Value.ChecklistCount,
            EquipmentCount = result.Value.EquipmentCount,
            SampleChecklistId = result.Value.SampleChecklistId,
            SampleChecklistCode = result.Value.SampleChecklistCode,
            EquipmentGroups = result.Value.Equipments
                .Select(MapEquipment)
                .GroupBy(equipment => string.IsNullOrWhiteSpace(equipment.CategoryName) ? "Sem categoria" : equipment.CategoryName!, PtBrComparer)
                .OrderBy(group => group.Key, PtBrComparer)
                .Select(group => new DashboardEquipmentGroupViewModel
                {
                    CategoryName = group.Key,
                    EquipmentCount = group.Count(),
                    Equipments = group
                        .OrderBy(equipment => equipment.EquipmentCode, PtBrComparer)
                        .ToList()
                })
                .ToList()
        };

        return View(model);
    }

    private static DashboardEquipmentStatusViewModel MapEquipment(Checklist.Application.Dtos.DashboardEquipmentStatusDto equipment)
    {
        var (label, cssClass) = equipment.Status switch
        {
            "ok" => ("Checklist OK", "text-bg-success"),
            "nok" => ("Checklist com NOK", "text-bg-danger"),
            _ => ("Checklist nao preenchido", "text-bg-secondary")
        };

        return new DashboardEquipmentStatusViewModel
        {
            EquipmentId = equipment.EquipmentId,
            EquipmentCode = equipment.EquipmentCode,
            EquipmentDescription = equipment.EquipmentDescription,
            CategoryName = equipment.CategoryName,
            StatusKey = equipment.Status,
            StatusLabel = label,
            StatusCssClass = cssClass,
            ChecklistId = equipment.ChecklistId,
            ChecklistCompletedAtDisplay = equipment.ChecklistCompletedAtUtc?.ToString("dd/MM/yyyy HH:mm")
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
