using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IOperatorChecklistService
{
    Task<Result<OperatorChecklistDraftDto>> GetDraftAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<Result<OperatorChecklistResultDto>> SubmitAsync(OperatorChecklistSubmissionDto request, CancellationToken cancellationToken = default);
}
