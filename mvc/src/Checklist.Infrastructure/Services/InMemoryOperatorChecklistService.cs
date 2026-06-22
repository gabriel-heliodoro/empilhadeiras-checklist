using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data.Models;

namespace Checklist.Infrastructure.Services;

internal class InMemoryOperatorChecklistService : IOperatorChecklistService
{
    private readonly ICurrentOperator _currentOperator;

    public InMemoryOperatorChecklistService(ICurrentOperator currentOperator)
    {
        _currentOperator = currentOperator;
    }

    public Task<Result<OperatorChecklistDraftDto>> GetDraftAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.Id.HasValue || !_currentOperator.SectorId.HasValue)
        {
            return Task.FromResult(Result<OperatorChecklistDraftDto>.Fail("Operator autenticado invalido."));
        }

        var equipment = SampleChecklistStore.OperatorEquipments.FirstOrDefault(x => x.Id == equipmentId && x.IsActive);
        if (equipment is null)
        {
            return Task.FromResult(Result<OperatorChecklistDraftDto>.Fail("Equipment nao encontrado ou inativo."));
        }

        return Task.FromResult(Result<OperatorChecklistDraftDto>.Ok(new OperatorChecklistDraftDto
        {
            Equipment = equipment,
            Operator = SampleChecklistStore.OperatorSession,
            ItemsTemplate = SampleChecklistStore.OperatorChecklistTemplates
                .Where(x => x.CategoryId == equipment.CategoryId && x.IsActive)
                .OrderBy(x => x.Order)
                .ToList()
        }));
    }

    public Task<Result<OperatorChecklistResultDto>> SubmitAsync(
        OperatorChecklistSubmissionDto request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.Id.HasValue || !_currentOperator.SectorId.HasValue)
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Operator autenticado invalido."));
        }

        if (_currentOperator.ForceChangePassword)
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("O operador precisa atualizar a senha antes de enviar o checklist."));
        }

        var equipment = SampleChecklistStore.OperatorEquipments.FirstOrDefault(x => x.Id == request.EquipmentId && x.IsActive);
        if (equipment is null)
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Equipment nao encontrado ou inativo."));
        }

        if (request.Items.Count == 0)
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Checklist deve ter pelo menos um item."));
        }

        if (request.Items.Any(x => string.IsNullOrWhiteSpace(x.Status) || string.Equals(x.Status, nameof(MvcItemStatus.NotChecked), StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Todos os itens devem ser respondidos antes do envio."));
        }

        var itemNokSemNotes = request.Items.FirstOrDefault(x =>
            string.Equals(x.Status, nameof(MvcItemStatus.NOK), StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(x.Notes));

        if (itemNokSemNotes is not null)
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Items marcados como NOK exigem observacao obrigatoria."));
        }

        if (string.IsNullOrWhiteSpace(request.OperatorSignatureBase64))
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("A assinatura do operador e obrigatoria."));
        }

        var submittedAtUtc = DateTime.UtcNow;
        if (SampleChecklistStore.HasChecklistForEquipmentOnDate(equipment.Code, submittedAtUtc))
        {
            return Task.FromResult(Result<OperatorChecklistResultDto>.Fail("Ja existe um checklist enviado hoje para este equipamento."));
        }

        var result = SampleChecklistStore.RegisterOperatorChecklist(
            equipment,
            SampleChecklistStore.OperatorSession,
            SampleChecklistStore.OperatorChecklistTemplates
                .Where(x => x.CategoryId == equipment.CategoryId && x.IsActive)
                .OrderBy(x => x.Order)
                .ToList(),
            request,
            submittedAtUtc);

        return Task.FromResult(Result<OperatorChecklistResultDto>.Ok(result));
    }
}
