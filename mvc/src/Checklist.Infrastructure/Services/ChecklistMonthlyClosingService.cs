using System.Security.Cryptography;
using System.Text.Json;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class ChecklistMonthlyClosingService
{
    private readonly AppDbContext _dbContext;
    private readonly ChecklistMonthlyWorkbookService _workbookService;

    public ChecklistMonthlyClosingService(AppDbContext dbContext, ChecklistMonthlyWorkbookService workbookService)
    {
        _dbContext = dbContext;
        _workbookService = workbookService;
    }

    public async Task<List<MvcMonthlyChecklistClosure>> ListAsync(
        Guid sectorId,
        int? year,
        int? month,
        Guid? equipmentId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MonthlyChecklistClosures
            .AsNoTracking()
            .Include(x => x.Equipment)
            .Include(x => x.ClosedBySupervisor)
            .Where(x => x.SectorId == sectorId);

        if (year is not null)
        {
            query = query.Where(x => x.Year == year.Value);
        }

        if (month is not null)
        {
            query = query.Where(x => x.Month == month.Value);
        }

        if (equipmentId.HasValue && equipmentId.Value != Guid.Empty)
        {
            query = query.Where(x => x.EquipmentId == equipmentId.Value);
        }

        return await query
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenBy(x => x.Equipment.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChecklistMonthlySnapshot> BuildPreviewAsync(
        Guid sectorId,
        Guid equipmentId,
        int year,
        int month,
        bool preferSnapshot = true,
        CancellationToken cancellationToken = default)
    {
        var existing = preferSnapshot
            ? await _dbContext.MonthlyChecklistClosures
                .AsNoTracking()
                .Include(x => x.Equipment)
                .Include(x => x.Sector)
                .FirstOrDefaultAsync(
                    x => x.SectorId == sectorId && x.EquipmentId == equipmentId && x.Year == year && x.Month == month,
                    cancellationToken)
            : null;

        if (existing is not null)
        {
            return Deserialize(existing.SnapshotJson);
        }

        return await BuildLiveSnapshotAsync(sectorId, equipmentId, year, month, cancellationToken);
    }

    public async Task<MvcMonthlyChecklistClosure> CloseAsync(
        Guid sectorId,
        Guid supervisorId,
        Guid equipmentId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.MonthlyChecklistClosures
            .FirstOrDefaultAsync(
                x => x.SectorId == sectorId && x.EquipmentId == equipmentId && x.Year == year && x.Month == month,
                cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var snapshot = await BuildLiveSnapshotAsync(sectorId, equipmentId, year, month, cancellationToken);
        var workbookBytes = _workbookService.Generate(snapshot);
        var hash = Convert.ToHexString(SHA256.HashData(workbookBytes));
        var fileName = $"CheckFlow_{snapshot.EquipmentCode}_{year}-{month:00}.xlsx";

        var closure = new MvcMonthlyChecklistClosure
        {
            SectorId = sectorId,
            EquipmentId = equipmentId,
            ClosedBySupervisorId = supervisorId,
            Year = year,
            Month = month,
            TemplateName = "Checklist - Empilhadeiras",
            TemplateVersion = snapshot.MonthlyClosureModel switch
            {
                MvcMonthlyClosureModel.CombustionForklift => "F_PTV_0208_a",
                MvcMonthlyClosureModel.ElectricForklift => "F_PTV_0208_b",
                _ => "v1"
            },
            SnapshotJson = JsonSerializer.Serialize(snapshot),
            PdfFileName = fileName,
            PdfSha256Hash = hash,
            PdfContent = workbookBytes,
            ChecklistCount = snapshot.TotalChecklistsConsidered,
            ClosedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var day in snapshot.Days)
        {
            closure.Checklists.Add(new MvcMonthlyChecklistClosureChecklist
            {
                ChecklistId = day.ChecklistId
            });
        }

        _dbContext.MonthlyChecklistClosures.Add(closure);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return closure;
    }

    public async Task<MvcMonthlyChecklistClosure?> GetClosureAsync(Guid sectorId, Guid closureId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MonthlyChecklistClosures
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == closureId && x.SectorId == sectorId, cancellationToken);
    }

    private async Task<ChecklistMonthlySnapshot> BuildLiveSnapshotAsync(
        Guid sectorId,
        Guid equipmentId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Sector)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == equipmentId && x.SectorId == sectorId, cancellationToken);

        if (equipment is null)
        {
            throw new InvalidOperationException("Equipamento nao encontrado para este setor.");
        }

        if (equipment.Category.MonthlyClosureModel == MvcMonthlyClosureModel.None)
        {
            throw new InvalidOperationException("A categoria deste equipamento nao possui modelo de fechamento mensal configurado.");
        }

        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var checklists = await _dbContext.Checklists
            .AsNoTracking()
            .Include(x => x.Operator)
            .Include(x => x.Items)
            .Where(x => x.SectorId == sectorId
                && x.EquipmentId == equipmentId
                && x.ReferenceDate >= start
                && x.ReferenceDate < end)
            .OrderBy(x => x.CompletedAt)
            .ToListAsync(cancellationToken);

        var templateItems = await _dbContext.ChecklistItemTemplates
            .AsNoTracking()
            .Where(x => x.CategoryId == equipment.CategoryId && x.IsActive)
            .OrderBy(x => x.Order)
            .Select(x => new
            {
                x.Order,
                x.Description
            })
            .ToListAsync(cancellationToken);

        var warnings = new List<string>();
        var groupedByDay = checklists
            .GroupBy(x => x.ReferenceDate.Day)
            .OrderBy(x => x.Key)
            .ToList();

        foreach (var group in groupedByDay.Where(x => x.Count() > 1))
        {
            warnings.Add($"Dia {group.Key:00}: encontrados {group.Count()} checklists. Foi considerado o ultimo do dia.");
        }

        var selected = groupedByDay
            .Select(group => group.OrderByDescending(x => x.CompletedAt).First())
            .OrderBy(x => x.ReferenceDate)
            .ToList();

        var missingDays = Enumerable.Range(1, daysInMonth)
            .Except(selected.Select(x => x.ReferenceDate.Day))
            .ToList();

        if (missingDays.Count > 0)
        {
            warnings.Add($"Dias sem checklist: {string.Join(", ", missingDays.Select(x => x.ToString("00")))}.");
        }

        var rowDefinitions = templateItems.Count > 0
            ? templateItems
            : selected
                .SelectMany(x => x.Items)
                .GroupBy(x => x.Order)
                .OrderBy(x => x.Key)
                .Select(group => new
                {
                    Order = group.Key,
                    Description = group.Select(x => x.Description).FirstOrDefault() ?? string.Empty
                })
                .ToList();

        var rows = rowDefinitions.Select(row =>
        {
            var values = Enumerable.Repeat<string?>(null, 31).ToList();
            foreach (var checklist in selected)
            {
                var item = checklist.Items.FirstOrDefault(x => x.Order == row.Order);
                if (item is not null)
                {
                    values[checklist.ReferenceDate.Day - 1] = ToCellValue(item.Status);
                }
            }

            return new ChecklistMonthlyRowSnapshot
            {
                Order = row.Order,
                Description = row.Description,
                ValuesByDay = values
            };
        }).ToList();

        var comments = new List<string>();
        foreach (var checklist in selected)
        {
            var day = checklist.ReferenceDate.Day;
            var operatorDisplay = $"{checklist.Operator.Registration} - {checklist.Operator.Name} {checklist.Operator.LastName}".Trim();

            if (!string.IsNullOrWhiteSpace(checklist.GeneralNotes))
            {
                comments.Add($"Dia {day:00} - {operatorDisplay} - Observacoes gerais: {checklist.GeneralNotes}");
            }

            foreach (var item in checklist.Items.Where(x => x.Status == MvcItemStatus.NOK))
            {
                var detail = string.IsNullOrWhiteSpace(item.Notes) ? "Sem detalhe informado." : item.Notes!;
                comments.Add($"Dia {day:00} - {operatorDisplay} - Item {item.Order} - {item.Description} - NAO OK - {detail}");
            }
        }

        var operators = selected
            .Select(x => $"{x.Operator.Name} {x.Operator.LastName} / {x.Operator.Registration}".Trim())
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return new ChecklistMonthlySnapshot
        {
            EquipmentId = equipment.Id,
            CategoryId = equipment.CategoryId,
            CategoryName = equipment.Category.Name,
            MonthlyClosureModel = equipment.Category.MonthlyClosureModel,
            EquipmentCode = equipment.Code,
            EquipmentDescription = equipment.Description,
            SectorName = equipment.Sector.Name,
            Year = year,
            Month = month,
            DaysInMonth = daysInMonth,
            TotalDaysWithChecklist = selected.Count,
            TotalChecklistsConsidered = selected.Count,
            Rows = rows,
            Days = selected.Select(x => new ChecklistMonthlyDaySnapshot
            {
                Day = x.ReferenceDate.Day,
                ChecklistId = x.Id,
                OperatorName = $"{x.Operator.Name} {x.Operator.LastName}".Trim(),
                OperatorRegistration = x.Operator.Registration,
                CompletedAt = x.CompletedAt
            }).ToList(),
            Comments = comments,
            ConsolidatedOperators = operators,
            Warnings = warnings
        };
    }

    private static string ToCellValue(MvcItemStatus status)
    {
        return status switch
        {
            MvcItemStatus.OK => "V",
            MvcItemStatus.NOK => "X",
            MvcItemStatus.NA => "|",
            _ => string.Empty
        };
    }

    private static ChecklistMonthlySnapshot Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ChecklistMonthlySnapshot>(json)
            ?? throw new InvalidOperationException("Snapshot mensal invalido.");
    }
}
