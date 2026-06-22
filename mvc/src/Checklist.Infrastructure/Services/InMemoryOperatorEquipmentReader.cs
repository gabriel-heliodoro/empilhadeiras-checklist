using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

internal class InMemoryOperatorEquipmentReader : IOperatorEquipmentReader
{
    public Task<Result<IReadOnlyList<OperatorEquipmentDto>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Task.FromResult(Result<IReadOnlyList<OperatorEquipmentDto>>.Fail("Informe um codigo ou QR ID para buscar o equipamento."));
        }

        var results = SampleChecklistStore.OperatorEquipments
            .Where(x =>
                x.Code.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || x.Description.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || x.QrId.ToString().Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Code)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<OperatorEquipmentDto>>.Ok(results));
    }

    public Task<Result<OperatorEquipmentDto>> GetByIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        var equipment = SampleChecklistStore.OperatorEquipments.FirstOrDefault(x => x.Id == equipmentId && x.IsActive);
        if (equipment is null)
        {
            return Task.FromResult(Result<OperatorEquipmentDto>.Fail("Equipment nao encontrado ou inativo."));
        }

        return Task.FromResult(Result<OperatorEquipmentDto>.Ok(equipment));
    }

    public Task<Result<OperatorEquipmentDto>> GetByQrIdAsync(Guid qrId, CancellationToken cancellationToken = default)
    {
        var equipment = SampleChecklistStore.OperatorEquipments.FirstOrDefault(x => x.QrId == qrId && x.IsActive);
        if (equipment is null)
        {
            return Task.FromResult(Result<OperatorEquipmentDto>.Fail("Equipment nao encontrado para este QR ID."));
        }

        return Task.FromResult(Result<OperatorEquipmentDto>.Ok(equipment));
    }
}
