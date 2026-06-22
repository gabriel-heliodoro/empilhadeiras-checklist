using Checklist.Application.Common;
using Checklist.Application.Dtos;

namespace Checklist.Application.Interfaces;

public interface IStpDocumentControlService
{
    Task<Result<IReadOnlyList<StpCompanySummaryDto>>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    Task<Result<StpCompanyDetailsDto>> GetCompanyDetailsAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Result> SaveCompanyAsync(StpCompanyUpsertDto request, CancellationToken cancellationToken = default);
    Task<Result<StpEmployeeDetailsDto>> GetEmployeeDetailsAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<Result> SaveEmployeeAsync(StpEmployeeUpsertDto request, CancellationToken cancellationToken = default);
    Task<Result> UploadCompanyDocumentAsync(Guid companyId, StpDocumentUploadDto request, CancellationToken cancellationToken = default);
    Task<Result> UploadEmployeeDocumentAsync(Guid employeeId, StpDocumentUploadDto request, CancellationToken cancellationToken = default);
    Task<Result<StpDocumentFileContentDto>> GetCompanyDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<Result<StpDocumentFileContentDto>> GetEmployeeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
}
