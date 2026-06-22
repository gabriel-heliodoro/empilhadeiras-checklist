using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class DbNonOkWorkflowService : INonOkWorkflowService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DbNonOkWorkflowService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<NonOkResponsibleOptionDto>>> ListResponsibleOptionsAsync(CancellationToken cancellationToken = default)
    {
        var options = await _dbContext.SupervisorUsers
            .AsNoTracking()
            .Include(user => user.Sector)
            .Where(user => user.IsActive && !user.IsMaster && user.Sector.IsActive)
            .OrderBy(user => user.Sector.Name)
            .ThenBy(user => user.Name)
            .ThenBy(user => user.LastName)
            .Select(user => new NonOkResponsibleOptionDto
            {
                Id = user.Id,
                FullName = $"{user.Name} {user.LastName}",
                Login = user.Login,
                SectorId = user.SectorId,
                SectorName = user.Sector.Name
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<NonOkResponsibleOptionDto>>.Ok(options);
    }

    public async Task<Result<NonOkPanelItemDto>> AssignAsync(Guid checklistItemId, NonOkAssignRequestDto request, CancellationToken cancellationToken = default)
    {
        var context = GetContext();
        if (!context.success)
        {
            return Result<NonOkPanelItemDto>.Fail(context.error!);
        }

        var item = await _dbContext.ChecklistItems
            .Include(entry => entry.Checklist)
                .ThenInclude(checklist => checklist.Sector)
            .Include(entry => entry.Checklist)
                .ThenInclude(checklist => checklist.Equipment)
            .Include(entry => entry.Checklist)
                .ThenInclude(checklist => checklist.Operator)
            .Include(entry => entry.Action)
            .FirstOrDefaultAsync(entry => entry.Id == checklistItemId, cancellationToken);

        if (item is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado.");
        }

        if (item.Checklist.SectorId != context.setorId)
        {
            return Result<NonOkPanelItemDto>.Fail("Supervisor sem permissao para atribuir este item.");
        }

        if (item.Status != MvcItemStatus.NOK)
        {
            return Result<NonOkPanelItemDto>.Fail("Apenas itens NOK podem virar tratativas.");
        }

        if (item.Action is not null)
        {
            return Result<NonOkPanelItemDto>.Fail("Este item ja possui uma tratativa registrada.");
        }

        var responsavel = await ResolveResponsibleAsync(request.ResponsibleSupervisorId, cancellationToken);
        if (responsavel is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Responsavel informado nao esta disponivel para atribuicao.");
        }

        var acao = new MvcChecklistItemAction
        {
            Id = Guid.NewGuid(),
            ChecklistItemId = item.Id,
            Status = MvcChecklistItemActionStatus.InProgress,
            ApprovedBySupervisorId = context.supervisorId!.Value,
            ApprovedAt = DateTime.UtcNow,
            ResponsibleSupervisorId = responsavel.Id,
            ResponsibleSectorId = responsavel.SectorId,
            AssignmentNotes = NormalizeOptionalText(request.AssignmentObservation),
            ResponsibleNotes = NormalizeOptionalText(request.ResponsibleObservation),
            PlannedCompletionDate = NormalizeDateOnly(request.PlannedCompletionDate),
            CompletionPercentage = NormalizeCompletionPercent(request.CompletionPercent),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ChecklistItemActions.Add(acao);
        _dbContext.ChecklistItemActionHistoryEntries.Add(CreateHistoryEntry(
            acao.Id,
            context.supervisorId.Value,
            "Tratativa atribuida",
            $"Responsavel definido: {responsavel.Name} {responsavel.LastName} ({responsavel.Sector.Name})."));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ReloadItemAsync(item.Id, context.setorId, context.supervisorId, cancellationToken);
    }

    public async Task<Result<NonOkPanelItemDto>> UpdateAsync(Guid checklistItemId, NonOkUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        var context = GetContext();
        if (!context.success)
        {
            return Result<NonOkPanelItemDto>.Fail(context.error!);
        }

        var item = await _dbContext.ChecklistItems
            .Include(entry => entry.Checklist)
            .Include(entry => entry.Action)
            .FirstOrDefaultAsync(entry => entry.Id == checklistItemId, cancellationToken);

        if (item is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado.");
        }

        if (item.Status != MvcItemStatus.NOK)
        {
            return Result<NonOkPanelItemDto>.Fail("Apenas itens NOK podem ser editados neste fluxo.");
        }

        if (item.Action is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Este item ainda nao possui tratativa para ser editada.");
        }

        if (item.Action.Status == MvcChecklistItemActionStatus.Completed)
        {
            return Result<NonOkPanelItemDto>.Fail("Tratativas concluidas nao podem ser editadas.");
        }

        var podeEditar = item.Checklist.SectorId == context.setorId ||
                         item.Action.ResponsibleSupervisorId == context.supervisorId ||
                         item.Action.ResponsibleSectorId == context.setorId;

        if (!podeEditar)
        {
            return Result<NonOkPanelItemDto>.Fail("Supervisor sem permissao para editar esta tratativa.");
        }

        var responsavel = await ResolveResponsibleAsync(request.ResponsibleSupervisorId, cancellationToken);
        if (responsavel is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Responsavel informado nao esta disponivel para atribuicao.");
        }

        var changes = new List<string>();
        var normalizedObservation = NormalizeOptionalText(request.ResponsibleObservation);
        var normalizedDate = NormalizeDateOnly(request.PlannedCompletionDate);
        var normalizedPercent = NormalizeCompletionPercent(request.CompletionPercent);

        if (item.Action.ResponsibleSupervisorId != responsavel.Id)
        {
            changes.Add($"Responsavel alterado para {responsavel.Name} {responsavel.LastName} ({responsavel.Sector.Name}).");
            item.Action.ResponsibleSupervisorId = responsavel.Id;
            item.Action.ResponsibleSectorId = responsavel.SectorId;
        }

        if (!string.Equals(item.Action.ResponsibleNotes, normalizedObservation, StringComparison.Ordinal))
        {
            changes.Add($"Notes do responsavel alterada para {DescribeOptional(normalizedObservation)}.");
            item.Action.ResponsibleNotes = normalizedObservation;
        }

        if (item.Action.PlannedCompletionDate != normalizedDate)
        {
            changes.Add($"Data prevista alterada para {FormatHistoryDate(normalizedDate)}.");
            item.Action.PlannedCompletionDate = normalizedDate;
        }

        if (item.Action.CompletionPercentage != normalizedPercent)
        {
            changes.Add($"Percentual de conclusao alterado para {normalizedPercent}%.");
            item.Action.CompletionPercentage = normalizedPercent;
        }

        if (changes.Count == 0)
        {
            return await ReloadItemAsync(item.Id, context.setorId, context.supervisorId, cancellationToken);
        }

        _dbContext.ChecklistItemActionHistoryEntries.Add(CreateHistoryEntry(
            item.Action.Id,
            context.supervisorId!.Value,
            "Tratativa atualizada",
            string.Join(Environment.NewLine, changes)));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ReloadItemAsync(item.Id, context.setorId, context.supervisorId, cancellationToken);
    }

    public async Task<Result<NonOkPanelItemDto>> CompleteAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        var context = GetContext();
        if (!context.success)
        {
            return Result<NonOkPanelItemDto>.Fail(context.error!);
        }

        var item = await _dbContext.ChecklistItems
            .Include(entry => entry.Checklist)
            .Include(entry => entry.Action)
            .FirstOrDefaultAsync(entry => entry.Id == checklistItemId, cancellationToken);

        if (item is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado.");
        }

        if (item.Status != MvcItemStatus.NOK)
        {
            return Result<NonOkPanelItemDto>.Fail("Apenas itens NOK podem ser concluidos neste fluxo.");
        }

        var podeConcluir = item.Checklist.SectorId == context.setorId ||
                           item.Action?.ResponsibleSupervisorId == context.supervisorId ||
                           item.Action?.ResponsibleSectorId == context.setorId;

        if (!podeConcluir)
        {
            return Result<NonOkPanelItemDto>.Fail("Supervisor sem permissao para concluir esta tratativa.");
        }

        if (item.Action is null)
        {
            var acao = new MvcChecklistItemAction
            {
                Id = Guid.NewGuid(),
                ChecklistItemId = item.Id,
                Status = MvcChecklistItemActionStatus.Completed,
                ApprovedBySupervisorId = context.supervisorId!.Value,
                ApprovedAt = DateTime.UtcNow,
                ResponsibleSupervisorId = context.supervisorId,
                ResponsibleSectorId = context.setorId,
                CompletionPercentage = 100,
                CompletedBySupervisorId = context.supervisorId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.ChecklistItemActions.Add(acao);
            _dbContext.ChecklistItemActionHistoryEntries.Add(CreateHistoryEntry(
                acao.Id,
                context.supervisorId.Value,
                "Tratativa concluida",
                "O item foi concluido diretamente, sem atribuicao previa."));
        }
        else
        {
            if (item.Action.Status == MvcChecklistItemActionStatus.Completed)
            {
                return Result<NonOkPanelItemDto>.Fail("Esta tratativa ja esta concluida.");
            }

            item.Action.Status = MvcChecklistItemActionStatus.Completed;
            item.Action.CompletionPercentage = 100;
            item.Action.CompletedBySupervisorId = context.supervisorId;
            item.Action.CompletedAt = DateTime.UtcNow;

            if (item.Action.ResponsibleSupervisorId is null)
            {
                item.Action.ResponsibleSupervisorId = context.supervisorId;
                item.Action.ResponsibleSectorId = context.setorId;
            }

            _dbContext.ChecklistItemActionHistoryEntries.Add(CreateHistoryEntry(
                item.Action.Id,
                context.supervisorId!.Value,
                "Tratativa concluida",
                $"Tratativa marcada como concluida com percentual final de {item.Action.CompletionPercentage}%."));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await ReloadItemAsync(item.Id, context.setorId, context.supervisorId, cancellationToken);
    }

    private async Task<Result<NonOkPanelItemDto>> ReloadItemAsync(Guid checklistItemId, Guid setorId, Guid? supervisorId, CancellationToken cancellationToken)
    {
        var item = await NonOkReadModel.BuildPanelQuery(_dbContext, setorId, supervisorId)
            .FirstOrDefaultAsync(entry => entry.Id == checklistItemId, cancellationToken);

        if (item is null)
        {
            return Result<NonOkPanelItemDto>.Fail("Nao foi possivel recarregar o item apos a operacao.");
        }

        return Result<NonOkPanelItemDto>.Ok(NonOkReadModel.MapItem(item));
    }

    private async Task<MvcSupervisorUser?> ResolveResponsibleAsync(Guid responsibleSupervisorId, CancellationToken cancellationToken)
    {
        return await _dbContext.SupervisorUsers
            .Include(user => user.Sector)
            .FirstOrDefaultAsync(user => user.Id == responsibleSupervisorId && user.IsActive && !user.IsMaster && user.Sector.IsActive, cancellationToken);
    }

    private (bool success, Guid? supervisorId, Guid setorId, string? error) GetContext()
    {
        if (!_currentUser.SectorId.HasValue)
        {
            return (false, null, Guid.Empty, "Sector do supervisor atual nao foi resolvido.");
        }

        if (!_currentUser.Id.HasValue)
        {
            return (false, null, Guid.Empty, "Supervisor atual nao foi resolvido.");
        }

        return (true, _currentUser.Id.Value, _currentUser.SectorId.Value, null);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime? NormalizeDateOnly(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var date = value.Value;
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static int NormalizeCompletionPercent(int value)
    {
        return Math.Clamp(value, 0, 100);
    }

    private static string DescribeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "sem observacao" : value;
    }

    private static string FormatHistoryDate(DateTime? value)
    {
        return value?.ToString("dd/MM/yyyy") ?? "sem data";
    }

    private static MvcChecklistItemActionHistory CreateHistoryEntry(Guid acaoId, Guid supervisorId, string title, string description)
    {
        return new MvcChecklistItemActionHistory
        {
            Id = Guid.NewGuid(),
            ChecklistItemActionId = acaoId,
            CreatedBySupervisorId = supervisorId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}

