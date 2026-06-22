using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

internal class UnavailableStpDocumentControlService : IStpDocumentControlService
{
    private const string Message = "Controle documental STP indisponivel sem conexao com banco de dados.";

    public Task<Result<IReadOnlyList<StpCompanySummaryDto>>> GetCompaniesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<IReadOnlyList<StpCompanySummaryDto>>.Fail(Message));

    public Task<Result<StpCompanyDetailsDto>> GetCompanyDetailsAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpCompanyDetailsDto>.Fail(Message));

    public Task<Result> SaveCompanyAsync(StpCompanyUpsertDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Fail(Message));

    public Task<Result<StpEmployeeDetailsDto>> GetEmployeeDetailsAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpEmployeeDetailsDto>.Fail(Message));

    public Task<Result> SaveEmployeeAsync(StpEmployeeUpsertDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Fail(Message));

    public Task<Result> UploadCompanyDocumentAsync(Guid companyId, StpDocumentUploadDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Fail(Message));

    public Task<Result> UploadEmployeeDocumentAsync(Guid employeeId, StpDocumentUploadDto request, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Fail(Message));

    public Task<Result<StpDocumentFileContentDto>> GetCompanyDocumentAsync(Guid documentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpDocumentFileContentDto>.Fail(Message));

    public Task<Result<StpDocumentFileContentDto>> GetEmployeeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result<StpDocumentFileContentDto>.Fail(Message));
}
