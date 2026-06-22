using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IOperatorEquipmentReader
{
    Task<Result<IReadOnlyList<OperatorEquipmentDto>>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<Result<OperatorEquipmentDto>> GetByIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<Result<OperatorEquipmentDto>> GetByQrIdAsync(Guid qrId, CancellationToken cancellationToken = default);
}
