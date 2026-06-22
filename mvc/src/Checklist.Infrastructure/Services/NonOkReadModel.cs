using Checklist.Application.Dtos;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

internal static class NonOkReadModel
{
    public static IQueryable<MvcChecklistItem> BuildPanelQuery(AppDbContext dbContext, Guid setorId, Guid? supervisorId)
    {
        return dbContext.ChecklistItems
            .AsNoTracking()
            .Include(item => item.Checklist)
                .ThenInclude(checklist => checklist.Sector)
            .Include(item => item.Checklist)
                .ThenInclude(checklist => checklist.Equipment)
            .Include(item => item.Checklist)
                .ThenInclude(checklist => checklist.Operator)
            .Include(item => item.Action!)
                .ThenInclude(acao => acao.ResponsibleSupervisor)
            .Include(item => item.Action!)
                .ThenInclude(acao => acao.ResponsibleSector)
            .Include(item => item.Action!)
                .ThenInclude(acao => acao.ApprovedBySupervisor)
            .Include(item => item.Action!)
                .ThenInclude(acao => acao.CompletedBySupervisor)
            .Include(item => item.Action!)
                .ThenInclude(acao => acao.History)
                    .ThenInclude(entry => entry.CreatedBySupervisor)
            .Where(item =>
                item.Status == MvcItemStatus.NOK &&
                (
                    (item.Checklist.SectorId == setorId && item.Action == null) ||
                    (item.Action != null &&
                     (item.Checklist.SectorId == setorId ||
                      (supervisorId.HasValue && item.Action.ResponsibleSupervisorId == supervisorId.Value) ||
                      item.Action.ResponsibleSectorId == setorId))
                ));
    }

    public static IQueryable<MvcChecklistItem> ApplyCommonFilters(IQueryable<MvcChecklistItem> query, NonOkFiltersDto filters)
    {
        if (DateTime.TryParse(filters.DataInicio, out var dataInicio))
        {
            var inicioUtc = new DateTime(dataInicio.Year, dataInicio.Month, dataInicio.Day, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(item => item.Checklist.ReferenceDate >= inicioUtc);
        }

        if (DateTime.TryParse(filters.DataFim, out var dataFim))
        {
            var fimUtc = new DateTime(dataFim.Year, dataFim.Month, dataFim.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            query = query.Where(item => item.Checklist.ReferenceDate < fimUtc);
        }

        if (!string.IsNullOrWhiteSpace(filters.Equipment))
        {
            var equipamento = filters.Equipment.Trim().ToLower();
            query = query.Where(item =>
                item.Checklist.Equipment.Code.ToLower().Contains(equipamento) ||
                item.Checklist.Equipment.Description.ToLower().Contains(equipamento));
        }

        if (!string.IsNullOrWhiteSpace(filters.Operator))
        {
            var operador = filters.Operator.Trim().ToLower();
            query = query.Where(item =>
                item.Checklist.Operator.Name.ToLower().Contains(operador) ||
                item.Checklist.Operator.Registration.Contains(filters.Operator));
        }

        return query;
    }

    public static NonOkPanelItemDto MapItem(MvcChecklistItem item)
    {
        var workflowStatus = item.Action is null
            ? "pending-approval"
            : item.Action.Status == MvcChecklistItemActionStatus.Completed
                ? "completed"
                : "in-progress";

        return new NonOkPanelItemDto
        {
            ChecklistId = item.ChecklistId,
            ChecklistItemId = item.Id,
            ChecklistCompletedAt = item.Checklist.CompletedAt,
            SourceSectorId = item.Checklist.SectorId,
            SourceSectorName = item.Checklist.Sector?.Name ?? "Sector nao informado",
            EquipmentCode = item.Checklist.Equipment?.Code ?? "Equipment nao informado",
            EquipmentDescription = item.Checklist.Equipment?.Description ?? "Description nao informada",
            OperatorName = item.Checklist.Operator?.Name ?? "Operator nao informado",
            OperatorRegistration = item.Checklist.Operator?.Registration ?? "-",
            Order = item.Order,
            Description = item.Description,
            Instruction = item.Instruction,
            Notes = item.Notes,
            NokImageBase64 = item.NokImageBase64,
            NokImageFileName = item.NokImageFileName,
            NokImageMimeType = item.NokImageMimeType,
            WorkflowStatus = workflowStatus,
            ResponsibleSupervisorId = item.Action?.ResponsibleSupervisorId,
            ResponsibleFullName = item.Action?.ResponsibleSupervisor is null
                ? null
                : $"{item.Action.ResponsibleSupervisor.Name} {item.Action.ResponsibleSupervisor.LastName}",
            ResponsibleSectorId = item.Action?.ResponsibleSectorId,
            ResponsibleSectorName = item.Action?.ResponsibleSector?.Name,
            AssignmentNotes = item.Action?.AssignmentNotes,
            ResponsibleNotes = item.Action?.ResponsibleNotes,
            PlannedCompletionDate = item.Action?.PlannedCompletionDate,
            CompletionPercentage = item.Action?.CompletionPercentage ?? 0,
            ApprovedAt = item.Action?.ApprovedAt,
            ApprovedByFullName = item.Action?.ApprovedBySupervisor is null
                ? null
                : $"{item.Action.ApprovedBySupervisor.Name} {item.Action.ApprovedBySupervisor.LastName}",
            WorkflowCompletedAt = item.Action?.CompletedAt,
            CompletedByFullName = item.Action?.CompletedBySupervisor is null
                ? null
                : $"{item.Action.CompletedBySupervisor.Name} {item.Action.CompletedBySupervisor.LastName}",
            History = item.Action?.History
                .OrderByDescending(entry => entry.CreatedAt)
                .Select(entry => new NonOkHistoryEntryDto
                {
                    Id = entry.Id,
                    Title = entry.Title,
                    Description = entry.Description,
                    CreatedAt = entry.CreatedAt,
                    CreatedByDisplayName = $"{entry.CreatedBySupervisor.Name} {entry.CreatedBySupervisor.LastName}"
                })
                .ToList() ?? []
        };
    }
}

