using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class DbNonOkReader : INonOkReader
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DbNonOkReader(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<NonOkPanelDto>> GetPanelAsync(NonOkFiltersDto filters, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.SectorId.HasValue)
        {
            return Result<NonOkPanelDto>.Fail("Sector do supervisor atual nao foi resolvido.");
        }

        var query = NonOkReadModel.BuildPanelQuery(_dbContext, _currentUser.SectorId.Value, _currentUser.Id);
        query = NonOkReadModel.ApplyCommonFilters(query, filters);

        var items = (await query
                .OrderByDescending(item => item.Checklist.CompletedAt)
                .ThenBy(item => item.Checklist.Equipment.Code)
                .ThenBy(item => item.Order)
                .ToListAsync(cancellationToken))
            .Select(NonOkReadModel.MapItem)
            .ToList();

        var dto = new NonOkPanelDto
        {
            PendingApproval = items.Where(item => item.WorkflowStatus == "pending-approval").ToList(),
            InProgress = items.Where(item => item.WorkflowStatus == "in-progress").ToList(),
            Completed = items.Where(item => item.WorkflowStatus == "completed").ToList()
        };

        return Result<NonOkPanelDto>.Ok(dto);
    }

    public async Task<Result<NonOkPanelItemDto>> GetByIdAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.SectorId.HasValue)
        {
            return Result<NonOkPanelItemDto>.Fail("Sector do supervisor atual nao foi resolvido.");
        }

        var item = await NonOkReadModel.BuildPanelQuery(_dbContext, _currentUser.SectorId.Value, _currentUser.Id)
            .FirstOrDefaultAsync(entry => entry.Id == checklistItemId, cancellationToken);

        if (item is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Item non-compliant nao encontrado.");
        }

        return Result<NonOkPanelItemDto>.Ok(NonOkReadModel.MapItem(item));
    }
}

