using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class DbDashboardReader : IDashboardReader
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DbDashboardReader(AppDbContext db, ICurrentUser currentUser, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.SectorId.HasValue)
        {
            return Result<DashboardSummaryDto>.Fail("Usuário atual não está autenticado para consultar a dashboard.");
        }

        var setorId = _currentUser.SectorId.Value;
        var today = BusinessDate.TodayKeyUtc();

        var checklistCount = await _db.Checklists
            .AsNoTracking()
            .CountAsync(x => x.SectorId == setorId && x.ReferenceDate == today, cancellationToken);

        var equipmentCount = await _db.Equipment
            .AsNoTracking()
            .CountAsync(x => x.SectorId == setorId && x.IsActive, cancellationToken);

        var equipments = await _db.Equipment
            .AsNoTracking()
            .Where(x => x.SectorId == setorId && x.IsActive)
            .Include(x => x.Category)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var todayChecklists = await _db.Checklists
            .AsNoTracking()
            .Where(x => x.SectorId == setorId && x.ReferenceDate == today)
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);

        var equipmentStatuses = equipments
            .Select(equipment =>
            {
                var checklist = todayChecklists
                    .Where(x => x.EquipmentId == equipment.Id)
                    .OrderByDescending(x => x.CompletedAt)
                    .FirstOrDefault();

                return new DashboardEquipmentStatusDto
                {
                    EquipmentId = equipment.Id,
                    EquipmentCode = equipment.Code,
                    EquipmentDescription = equipment.Description,
                    CategoryName = equipment.Category?.Name,
                    Status = ResolveStatus(checklist),
                    ChecklistId = checklist?.Id,
                    ChecklistCompletedAtUtc = checklist?.CompletedAt
                };
            })
            .ToList();

        var latestChecklist = await _db.Checklists
            .AsNoTracking()
            .Where(x => x.SectorId == setorId)
            .Include(x => x.Equipment)
            .OrderByDescending(x => x.CompletedAt)
            .Select(x => new
            {
                x.Id,
                Code = x.Equipment.Code
            })
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new DashboardSummaryDto
        {
            UserDisplayName = _currentUser.UserName ?? "Supervisor",
            CurrentUtcTimestamp = _dateTimeProvider.CurrentUtcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            ChecklistCount = checklistCount,
            EquipmentCount = equipmentCount,
            SampleChecklistId = latestChecklist?.Id,
            SampleChecklistCode = latestChecklist?.Code,
            Equipments = equipmentStatuses
        };

        return Result<DashboardSummaryDto>.Ok(dto);
    }

    private static string ResolveStatus(MvcChecklist? checklist)
    {
        if (checklist is null)
        {
            return "nao-preenchido";
        }

        return checklist.Items.Any(item => item.Status == MvcItemStatus.NOK)
            ? "nok"
            : "ok";
    }
}
