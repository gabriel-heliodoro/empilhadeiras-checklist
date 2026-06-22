using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class DbChecklistReader : IChecklistReader
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DbChecklistReader(AppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<ChecklistDetailsDto>> GetByIdAsync(Guid checklistId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.SectorId.HasValue)
        {
            return Result<ChecklistDetailsDto>.Fail("Usuario atual nao esta autenticado para consultar checklists.");
        }

        var setorId = _currentUser.SectorId.Value;

        var checklist = await _db.Checklists
            .AsNoTracking()
            .Include(x => x.Equipment)
            .ThenInclude(x => x.Category)
            .Include(x => x.Operator)
            .Include(x => x.Sector)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == checklistId && x.SectorId == setorId, cancellationToken);

        if (checklist is null)
        {
            return Result<ChecklistDetailsDto>.Fail("Checklist nao encontrado para o setor do usuario atual.");
        }

        var dto = new ChecklistDetailsDto
        {
            Id = checklist.Id,
            Code = checklist.Equipment.Code,
            EquipmentCode = checklist.Equipment.Code,
            EquipmentDescription = checklist.Equipment.Description,
            OperatorName = checklist.Operator.Name,
            SectorName = checklist.Sector.Name,
            Status = ToStatusLabel(checklist.Status, checklist.Items),
            CreatedAtUtc = checklist.CreatedAt,
            Items = checklist.Items
                .OrderBy(x => x.Order)
                .Select(item => new ChecklistItemDto
                {
                    Label = item.Description,
                    Status = item.Status.ToString(),
                    Notes = item.Notes
                })
                .ToList()
        };

        return Result<ChecklistDetailsDto>.Ok(dto);
    }

    public async Task<Result<IReadOnlyList<ChecklistListItemDto>>> ListAsync(
        ChecklistListFiltersDto filters,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.SectorId.HasValue)
        {
            return Result<IReadOnlyList<ChecklistListItemDto>>.Fail("Usuario atual nao esta autenticado para consultar checklists.");
        }

        var setorId = _currentUser.SectorId.Value;

        var query = _db.Checklists
            .AsNoTracking()
            .Where(x => x.SectorId == setorId)
            .Include(x => x.Equipment)
            .Include(x => x.Operator)
            .Include(x => x.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.DataInicio) && DateTime.TryParse(filters.DataInicio, out var dataInicio))
        {
            var inicioUtc = new DateTime(dataInicio.Year, dataInicio.Month, dataInicio.Day, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(x => x.ReferenceDate >= inicioUtc);
        }

        if (!string.IsNullOrWhiteSpace(filters.DataFim) && DateTime.TryParse(filters.DataFim, out var dataFim))
        {
            var fimUtc = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            query = query.Where(x => x.ReferenceDate < fimUtc);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var normalizedStatus = filters.Status.Trim().ToLowerInvariant();
            if (normalizedStatus is "ok" or "nok")
            {
                var statusOk = normalizedStatus == "ok";
                query = query.Where(x => x.IsApproved == statusOk);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.Operator))
        {
            var operador = filters.Operator.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Operator.Name.ToLower().Contains(operador) ||
                x.Operator.Registration.Contains(filters.Operator));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ChecklistListItemDto
            {
                Id = x.Id,
                SectorId = x.SectorId,
                EquipmentCode = x.Equipment.Code,
                EquipmentDescription = x.Equipment.Description,
                OperatorName = x.Operator.Name,
                OperatorRegistration = x.Operator.Registration,
                CreatedAt = x.CreatedAt,
                Status = x.IsApproved ? "ok" : "nok",
                TotalItems = x.Items.Count,
                ItemsOk = x.Items.Count(i => i.Status == MvcItemStatus.OK),
                ItemsNok = x.Items.Count(i => i.Status == MvcItemStatus.NOK)
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChecklistListItemDto>>.Ok(items);
    }

    private static string ToStatusLabel(MvcChecklistStatus status, IReadOnlyCollection<MvcChecklistItem> items)
    {
        if (items.Any(x => x.Status == MvcItemStatus.NOK))
        {
            return "Nao conforme";
        }

        return status switch
        {
            MvcChecklistStatus.Approved => "Compliant",
            MvcChecklistStatus.Rejected => "Non-compliant",
            MvcChecklistStatus.Completed => "Completed",
            MvcChecklistStatus.UnderMaintenance => "Under maintenance",
            _ => "Under review"
        };
    }
}
