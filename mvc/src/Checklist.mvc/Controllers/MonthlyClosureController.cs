using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Services;
using Checklist.Mvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Mvc.Controllers;

[Authorize(Policy = "SectorSupervisorReady")]
public class MonthlyClosureController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ChecklistMonthlyClosingService _closingService;

    public MonthlyClosureController(
        AppDbContext dbContext,
        ICurrentUser currentUser,
        ChecklistMonthlyClosingService closingService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _closingService = closingService;
    }

    [HttpGet("monthly-closures")]
    public async Task<IActionResult> Index([FromQuery] MonthlyClosureFilterViewModel filters, CancellationToken cancellationToken)
    {
        var sectorId = _currentUser.SectorId;
        var supervisorId = _currentUser.Id;
        if (sectorId is null || supervisorId is null)
        {
            return Forbid();
        }

        var equipmentOptions = await GetEquipmentOptionsAsync(sectorId.Value, cancellationToken);
        if (!filters.EquipmentId.HasValue && equipmentOptions.Count > 0)
        {
            filters.EquipmentId = equipmentOptions[0].Id;
        }

        var closures = await _closingService.ListAsync(sectorId.Value, filters.Year, filters.Month, filters.EquipmentId, cancellationToken);

        var model = new MonthlyClosurePageViewModel
        {
            Filters = filters,
            EquipmentOptions = equipmentOptions,
            Closures = closures.Select(closure => new MonthlyClosureSummaryItemViewModel
            {
                Id = closure.Id,
                EquipmentId = closure.EquipmentId,
                EquipmentCode = closure.Equipment.Code,
                EquipmentDescription = closure.Equipment.Description,
                Year = closure.Year,
                Month = closure.Month,
                ChecklistCount = closure.ChecklistCount,
                TemplateVersion = closure.TemplateVersion,
                FileName = closure.PdfFileName,
                ClosedAt = closure.ClosedAt,
                ClosedByName = $"{closure.ClosedBySupervisor.Name} {closure.ClosedBySupervisor.LastName}".Trim()
            }).ToList()
        };

        if (filters.EquipmentId.HasValue && filters.EquipmentId.Value != Guid.Empty)
        {
            try
            {
                var snapshot = await _closingService.BuildPreviewAsync(
                    sectorId.Value,
                    filters.EquipmentId.Value,
                    filters.Year,
                    filters.Month,
                    cancellationToken: cancellationToken);

                var existingClosure = closures.FirstOrDefault(x => x.EquipmentId == filters.EquipmentId.Value && x.Year == filters.Year && x.Month == filters.Month);
                model.Preview = new MonthlyClosurePreviewViewModel
                {
                    IsAlreadyClosed = existingClosure is not null,
                    ClosureId = existingClosure?.Id,
                    EquipmentId = snapshot.EquipmentId,
                    EquipmentCode = snapshot.EquipmentCode,
                    EquipmentDescription = snapshot.EquipmentDescription,
                    SectorName = snapshot.SectorName,
                    Year = snapshot.Year,
                    Month = snapshot.Month,
                    TotalDaysWithChecklist = snapshot.TotalDaysWithChecklist,
                    TotalChecklistsConsidered = snapshot.TotalChecklistsConsidered,
                    Comments = snapshot.Comments,
                    ConsolidatedOperators = snapshot.ConsolidatedOperators,
                    Warnings = snapshot.Warnings,
                    Days = snapshot.Days.Select(x => new MonthlyClosureDayViewModel
                    {
                        Day = x.Day,
                        ChecklistId = x.ChecklistId,
                        OperatorName = x.OperatorName,
                        OperatorRegistration = x.OperatorRegistration,
                        CompletedAt = x.CompletedAt
                    }).ToList(),
                    Rows = snapshot.Rows.Select(x => new MonthlyClosureRowViewModel
                    {
                        Order = x.Order,
                        Description = x.Description,
                        ValuesByDay = x.ValuesByDay
                    }).ToList()
                };
            }
            catch (InvalidOperationException exception)
            {
                model.PreviewError = exception.Message;
            }
        }

        return View(model);
    }

    [HttpPost("monthly-closures/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(MonthlyClosureFilterViewModel filters, CancellationToken cancellationToken)
    {
        var sectorId = _currentUser.SectorId;
        var supervisorId = _currentUser.Id;
        if (sectorId is null || supervisorId is null)
        {
            return Forbid();
        }

        if (!filters.EquipmentId.HasValue || filters.EquipmentId == Guid.Empty)
        {
            TempData["StatusMessage"] = "Selecione um equipamento para fechar.";
            TempData["StatusType"] = "error";
            return RedirectToAction(nameof(Index), new { filters.Year, filters.Month });
        }

        try
        {
            var closure = await _closingService.CloseAsync(
                sectorId.Value,
                supervisorId.Value,
                filters.EquipmentId.Value,
                filters.Year,
                filters.Month,
                cancellationToken);

            TempData["StatusMessage"] = $"Fechamento mensal gerado: {closure.PdfFileName}.";
            TempData["StatusType"] = "success";
        }
        catch (InvalidOperationException exception)
        {
            TempData["StatusMessage"] = exception.Message;
            TempData["StatusType"] = "error";
        }

        return RedirectToAction(nameof(Index), new
        {
            equipmentId = filters.EquipmentId,
            year = filters.Year,
            month = filters.Month
        });
    }

    [HttpGet("monthly-closures/{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var sectorId = _currentUser.SectorId;
        if (sectorId is null)
        {
            return Forbid();
        }

        var closure = await _closingService.GetClosureAsync(sectorId.Value, id, cancellationToken);
        if (closure is null)
        {
            return NotFound();
        }

        return File(
            closure.PdfContent,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            closure.PdfFileName);
    }

    private async Task<List<ManagementOptionViewModel>> GetEquipmentOptionsAsync(Guid sectorId, CancellationToken cancellationToken)
    {
        return await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.SectorId == sectorId && x.Category.MonthlyClosureModel != Checklist.Infrastructure.Data.Models.MvcMonthlyClosureModel.None)
            .OrderBy(x => x.Code)
            .Select(x => new ManagementOptionViewModel
            {
                Id = x.Id,
                Label = $"{x.Code} - {x.Description}"
            })
            .ToListAsync(cancellationToken);
    }
}
