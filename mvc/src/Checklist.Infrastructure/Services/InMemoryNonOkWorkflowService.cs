using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

public class InMemoryNonOkWorkflowService : INonOkWorkflowService
{
    private static readonly object SyncRoot = new();

    public Task<Result<IReadOnlyList<NonOkResponsibleOptionDto>>> ListResponsibleOptionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<IReadOnlyList<NonOkResponsibleOptionDto>>.Ok(SampleChecklistStore.NonOkResponsibleOptions));
    }

    public Task<Result<NonOkPanelItemDto>> AssignAsync(Guid checklistItemId, NonOkAssignRequestDto request, CancellationToken cancellationToken = default)
    {
        lock (SyncRoot)
        {
            var item = SampleChecklistStore.NonOkItemState.FirstOrDefault(entry => entry.ChecklistItemId == checklistItemId);
            if (item is null)
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado."));
            }

            if (item.WorkflowStatus != "pending-approval")
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Este item ja possui uma tratativa registrada."));
            }

            var responsible = SampleChecklistStore.NonOkResponsibleOptions.FirstOrDefault(entry => entry.Id == request.ResponsibleSupervisorId);
            if (responsible is null)
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Responsavel informado nao esta disponivel para atribuicao."));
            }

            var updated = Clone(item, workflowStatus: "in-progress", responsibleName: responsible.FullName, responsibleSectorName: responsible.SectorName,
                assignmentObservation: NormalizeOptionalText(request.AssignmentObservation),
                responsibleObservation: NormalizeOptionalText(request.ResponsibleObservation),
                plannedDate: NormalizeDateOnly(request.PlannedCompletionDate),
                completionPercent: Math.Clamp(request.CompletionPercent, 0, 100),
                approvedBy: "Supervisor de teste",
                approvedAt: DateTime.UtcNow,
                historyEntry: new NonOkHistoryEntryDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Tratativa atribuida",
                    Description = $"Responsavel definido: {responsible.FullName} ({responsible.SectorName}).",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByDisplayName = "Supervisor de teste"
                },
                responsibleId: responsible.Id,
                responsibleSectorId: responsible.SectorId);

            Replace(updated);
            return Task.FromResult(Result<NonOkPanelItemDto>.Ok(updated));
        }
    }

    public Task<Result<NonOkPanelItemDto>> UpdateAsync(Guid checklistItemId, NonOkUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        lock (SyncRoot)
        {
            var item = SampleChecklistStore.NonOkItemState.FirstOrDefault(entry => entry.ChecklistItemId == checklistItemId);
            if (item is null)
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado."));
            }

            if (item.WorkflowStatus == "pending-approval")
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Este item ainda nao possui tratativa para ser editada."));
            }

            if (item.WorkflowStatus == "completed")
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Tratativas concluidas nao podem ser editadas."));
            }

            var responsible = SampleChecklistStore.NonOkResponsibleOptions.FirstOrDefault(entry => entry.Id == request.ResponsibleSupervisorId);
            if (responsible is null)
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Responsavel informado nao esta disponivel para atribuicao."));
            }

            var changes = new List<string>();
            if (item.ResponsibleSupervisorId != responsible.Id)
            {
                changes.Add($"Responsavel alterado para {responsible.FullName} ({responsible.SectorName}).");
            }

            var normalizedObservation = NormalizeOptionalText(request.ResponsibleObservation);
            var normalizedDate = NormalizeDateOnly(request.PlannedCompletionDate);
            var normalizedPercent = Math.Clamp(request.CompletionPercent, 0, 100);

            if (!string.Equals(item.ResponsibleNotes, normalizedObservation, StringComparison.Ordinal))
            {
                changes.Add("Notes do responsavel atualizada.");
            }

            if (item.PlannedCompletionDate != normalizedDate)
            {
                changes.Add("Data prevista atualizada.");
            }

            if (item.CompletionPercentage != normalizedPercent)
            {
                changes.Add($"Percentual atualizado para {normalizedPercent}%.");
            }

            var updated = Clone(item,
                responsibleName: responsible.FullName,
                responsibleSectorName: responsible.SectorName,
                responsibleObservation: normalizedObservation,
                plannedDate: normalizedDate,
                completionPercent: normalizedPercent,
                responsibleId: responsible.Id,
                responsibleSectorId: responsible.SectorId,
                historyEntry: changes.Count == 0 ? null : new NonOkHistoryEntryDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Tratativa atualizada",
                    Description = string.Join(Environment.NewLine, changes),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByDisplayName = "Supervisor de teste"
                });

            Replace(updated);
            return Task.FromResult(Result<NonOkPanelItemDto>.Ok(updated));
        }
    }

    public Task<Result<NonOkPanelItemDto>> CompleteAsync(Guid checklistItemId, CancellationToken cancellationToken = default)
    {
        lock (SyncRoot)
        {
            var item = SampleChecklistStore.NonOkItemState.FirstOrDefault(entry => entry.ChecklistItemId == checklistItemId);
            if (item is null)
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Item non-OK nao encontrado."));
            }

            if (item.WorkflowStatus == "completed")
            {
                return Task.FromResult(Result<NonOkPanelItemDto>.Fail("Esta tratativa ja esta concluida."));
            }

            var updated = Clone(item,
                workflowStatus: "completed",
                completionPercent: 100,
                approvedBy: item.ApprovedByFullName ?? "Supervisor de teste",
                approvedAt: item.ApprovedAt ?? DateTime.UtcNow,
                concludedBy: "Supervisor de teste",
                concludedAt: DateTime.UtcNow,
                historyEntry: new NonOkHistoryEntryDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Tratativa concluida",
                    Description = "Tratativa marcada como concluida com percentual final de 100%.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByDisplayName = "Supervisor de teste"
                });

            Replace(updated);
            return Task.FromResult(Result<NonOkPanelItemDto>.Ok(updated));
        }
    }

    private static void Replace(NonOkPanelItemDto updated)
    {
        var index = SampleChecklistStore.NonOkItemState.FindIndex(entry => entry.ChecklistItemId == updated.ChecklistItemId);
        if (index >= 0)
        {
            SampleChecklistStore.NonOkItemState[index] = updated;
        }
    }

    private static NonOkPanelItemDto Clone(
        NonOkPanelItemDto source,
        string? workflowStatus = null,
        string? responsibleName = null,
        string? responsibleSectorName = null,
        string? assignmentObservation = null,
        string? responsibleObservation = null,
        DateTime? plannedDate = null,
        int? completionPercent = null,
        string? approvedBy = null,
        DateTime? approvedAt = null,
        string? concludedBy = null,
        DateTime? concludedAt = null,
        NonOkHistoryEntryDto? historyEntry = null,
        Guid? responsibleId = null,
        Guid? responsibleSectorId = null)
    {
        var history = source.History.ToList();
        if (historyEntry is not null)
        {
            history.Insert(0, historyEntry);
        }

        return new NonOkPanelItemDto
        {
            ChecklistId = source.ChecklistId,
            ChecklistItemId = source.ChecklistItemId,
            ChecklistCompletedAt = source.ChecklistCompletedAt,
            SourceSectorId = source.SourceSectorId,
            SourceSectorName = source.SourceSectorName,
            EquipmentCode = source.EquipmentCode,
            EquipmentDescription = source.EquipmentDescription,
            OperatorName = source.OperatorName,
            OperatorRegistration = source.OperatorRegistration,
            Order = source.Order,
            Description = source.Description,
            Instruction = source.Instruction,
            Notes = source.Notes,
            NokImageBase64 = source.NokImageBase64,
            NokImageFileName = source.NokImageFileName,
            NokImageMimeType = source.NokImageMimeType,
            WorkflowStatus = workflowStatus ?? source.WorkflowStatus,
            ResponsibleSupervisorId = responsibleId ?? source.ResponsibleSupervisorId,
            ResponsibleFullName = responsibleName ?? source.ResponsibleFullName,
            ResponsibleSectorId = responsibleSectorId ?? source.ResponsibleSectorId,
            ResponsibleSectorName = responsibleSectorName ?? source.ResponsibleSectorName,
            AssignmentNotes = assignmentObservation ?? source.AssignmentNotes,
            ResponsibleNotes = responsibleObservation ?? source.ResponsibleNotes,
            PlannedCompletionDate = plannedDate ?? source.PlannedCompletionDate,
            CompletionPercentage = completionPercent ?? source.CompletionPercentage,
            ApprovedAt = approvedAt ?? source.ApprovedAt,
            ApprovedByFullName = approvedBy ?? source.ApprovedByFullName,
            WorkflowCompletedAt = concludedAt ?? source.WorkflowCompletedAt,
            CompletedByFullName = concludedBy ?? source.CompletedByFullName,
            History = history
        };
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
}

