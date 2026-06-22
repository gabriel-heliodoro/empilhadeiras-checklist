using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

internal class DbStpDocumentControlService : IStpDocumentControlService
{
    private const long MaxUploadSizeBytes = 15 * 1024 * 1024;

    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DbStpDocumentControlService(AppDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<StpCompanySummaryDto>>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<IReadOnlyList<StpCompanySummaryDto>>.Fail("Usuario sem setor vinculado.");
        }

        var companies = await _dbContext.StpCompanyDocuments
            .AsNoTracking()
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Name)
            .Select(x => new StpCompanySummaryDto
            {
                Id = x.Id,
                SectorId = x.SectorId,
                Name = x.Name,
                IsActive = x.IsActive,
                TotalDocuments = x.Documents.Count,
                TotalEmployees = x.Employees.Count
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StpCompanySummaryDto>>.Ok(companies);
    }

    public async Task<Result<StpCompanyDetailsDto>> GetCompanyDetailsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpCompanyDetailsDto>.Fail("Usuario sem setor vinculado.");
        }

        var company = await _dbContext.StpCompanyDocuments
            .AsNoTracking()
            .Include(x => x.Documents)
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == companyId && x.SectorId == sectorId, cancellationToken);

        if (company is null)
        {
            return Result<StpCompanyDetailsDto>.Fail("Empresa nao encontrada.");
        }

        return Result<StpCompanyDetailsDto>.Ok(new StpCompanyDetailsDto
        {
            Company = MapCompany(company),
            Documents = company.Documents
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapDocument)
                .ToList(),
            Employees = company.Employees
                .OrderBy(x => x.Name)
                .Select(MapEmployee)
                .ToList()
        });
    }

    public async Task<Result> SaveCompanyAsync(StpCompanyUpsertDto request, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result.Fail("Usuario sem setor vinculado.");
        }

        var normalizedName = NormalizeRequired(request.Name);
        if (normalizedName is null)
        {
            return Result.Fail("Informe o nome da empresa.");
        }

        var duplicated = await _dbContext.StpCompanyDocuments.AnyAsync(
            x => x.SectorId == sectorId
                && x.Name == normalizedName
                && (!request.Id.HasValue || x.Id != request.Id.Value),
            cancellationToken);

        if (duplicated)
        {
            return Result.Fail("Ja existe uma empresa com este nome no setor.");
        }

        if (request.Id.HasValue)
        {
            var company = await _dbContext.StpCompanyDocuments.FirstOrDefaultAsync(
                x => x.Id == request.Id.Value && x.SectorId == sectorId,
                cancellationToken);

            if (company is null)
            {
                return Result.Fail("Empresa nao encontrada.");
            }

            company.Name = normalizedName;
            company.IsActive = request.IsActive;
        }
        else
        {
            _dbContext.StpCompanyDocuments.Add(new MvcStpCompanyDocument
            {
                SectorId = sectorId,
                Name = normalizedName,
                IsActive = request.IsActive
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<StpEmployeeDetailsDto>> GetEmployeeDetailsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpEmployeeDetailsDto>.Fail("Usuario sem setor vinculado.");
        }

        var employee = await _dbContext.StpEmployeeDocuments
            .AsNoTracking()
            .Include(x => x.Company)
                .ThenInclude(x => x.Documents)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.Company.SectorId == sectorId, cancellationToken);

        if (employee is null)
        {
            return Result<StpEmployeeDetailsDto>.Fail("Funcionario nao encontrado.");
        }

        return Result<StpEmployeeDetailsDto>.Ok(new StpEmployeeDetailsDto
        {
            Company = MapCompany(employee.Company),
            Employee = MapEmployee(employee),
            Documents = employee.Documents
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapDocument)
                .ToList()
        });
    }

    public async Task<Result> SaveEmployeeAsync(StpEmployeeUpsertDto request, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result.Fail("Usuario sem setor vinculado.");
        }

        if (request.CompanyId == Guid.Empty)
        {
            return Result.Fail("Empresa invalida.");
        }

        var companyExists = await _dbContext.StpCompanyDocuments.AnyAsync(
            x => x.Id == request.CompanyId && x.SectorId == sectorId,
            cancellationToken);

        if (!companyExists)
        {
            return Result.Fail("Empresa nao encontrada.");
        }

        var normalizedName = NormalizeRequired(request.Name);
        if (normalizedName is null)
        {
            return Result.Fail("Informe o nome do funcionario.");
        }

        var duplicated = await _dbContext.StpEmployeeDocuments.AnyAsync(
            x => x.CompanyId == request.CompanyId
                && x.Name == normalizedName
                && (!request.Id.HasValue || x.Id != request.Id.Value),
            cancellationToken);

        if (duplicated)
        {
            return Result.Fail("Ja existe um funcionario com este nome nesta empresa.");
        }

        if (request.Id.HasValue)
        {
            var employee = await _dbContext.StpEmployeeDocuments
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.Company.SectorId == sectorId, cancellationToken);

            if (employee is null)
            {
                return Result.Fail("Funcionario nao encontrado.");
            }

            employee.Name = normalizedName;
            employee.Role = NormalizeOptional(request.Role);
            employee.IsActive = request.IsActive;
        }
        else
        {
            _dbContext.StpEmployeeDocuments.Add(new MvcStpEmployeeDocument
            {
                CompanyId = request.CompanyId,
                Name = normalizedName,
                Role = NormalizeOptional(request.Role),
                IsActive = request.IsActive
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> UploadCompanyDocumentAsync(Guid companyId, StpDocumentUploadDto request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Id.HasValue || !TryGetSectorId(out var sectorId))
        {
            return Result.Fail("Usuario autenticado invalido.");
        }

        var company = await _dbContext.StpCompanyDocuments.FirstOrDefaultAsync(
            x => x.Id == companyId && x.SectorId == sectorId,
            cancellationToken);

        if (company is null)
        {
            return Result.Fail("Empresa nao encontrada.");
        }

        var validationError = ValidateUpload(request);
        if (validationError is not null)
        {
            return Result.Fail(validationError);
        }

        _dbContext.StpCompanyDocumentFiles.Add(new MvcStpCompanyDocumentFile
        {
            CompanyId = companyId,
            Name = NormalizeDocumentName(request.Name, request.OriginalFileName),
            OriginalFileName = Path.GetFileName(request.OriginalFileName),
            MimeType = NormalizeMimeType(request.MimeType),
            SizeInBytes = request.SizeInBytes,
            Content = request.Content,
            UploadedBySupervisorId = _currentUser.Id.Value
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> UploadEmployeeDocumentAsync(Guid employeeId, StpDocumentUploadDto request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Id.HasValue || !TryGetSectorId(out var sectorId))
        {
            return Result.Fail("Usuario autenticado invalido.");
        }

        var employee = await _dbContext.StpEmployeeDocuments
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.Company.SectorId == sectorId, cancellationToken);

        if (employee is null)
        {
            return Result.Fail("Funcionario nao encontrado.");
        }

        var validationError = ValidateUpload(request);
        if (validationError is not null)
        {
            return Result.Fail(validationError);
        }

        _dbContext.StpEmployeeDocumentFiles.Add(new MvcStpEmployeeDocumentFile
        {
            EmployeeId = employeeId,
            Name = NormalizeDocumentName(request.Name, request.OriginalFileName),
            OriginalFileName = Path.GetFileName(request.OriginalFileName),
            MimeType = NormalizeMimeType(request.MimeType),
            SizeInBytes = request.SizeInBytes,
            Content = request.Content,
            UploadedBySupervisorId = _currentUser.Id.Value
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<StpDocumentFileContentDto>> GetCompanyDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpDocumentFileContentDto>.Fail("Usuario sem setor vinculado.");
        }

        var document = await _dbContext.StpCompanyDocumentFiles
            .AsNoTracking()
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == documentId && x.Company.SectorId == sectorId, cancellationToken);

        if (document is null)
        {
            return Result<StpDocumentFileContentDto>.Fail("Documento da empresa nao encontrado.");
        }

        return Result<StpDocumentFileContentDto>.Ok(new StpDocumentFileContentDto
        {
            FileName = document.OriginalFileName,
            MimeType = document.MimeType,
            Content = document.Content
        });
    }

    public async Task<Result<StpDocumentFileContentDto>> GetEmployeeDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpDocumentFileContentDto>.Fail("Usuario sem setor vinculado.");
        }

        var document = await _dbContext.StpEmployeeDocumentFiles
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == documentId && x.Employee.Company.SectorId == sectorId, cancellationToken);

        if (document is null)
        {
            return Result<StpDocumentFileContentDto>.Fail("Documento do funcionario nao encontrado.");
        }

        return Result<StpDocumentFileContentDto>.Ok(new StpDocumentFileContentDto
        {
            FileName = document.OriginalFileName,
            MimeType = document.MimeType,
            Content = document.Content
        });
    }

    private bool TryGetSectorId(out Guid sectorId)
    {
        sectorId = _currentUser.SectorId ?? Guid.Empty;
        return sectorId != Guid.Empty;
    }

    private static StpCompanySummaryDto MapCompany(MvcStpCompanyDocument company)
    {
        return new StpCompanySummaryDto
        {
            Id = company.Id,
            SectorId = company.SectorId,
            Name = company.Name,
            IsActive = company.IsActive,
            TotalDocuments = company.Documents.Count,
            TotalEmployees = company.Employees.Count
        };
    }

    private static StpEmployeeSummaryDto MapEmployee(MvcStpEmployeeDocument employee)
    {
        return new StpEmployeeSummaryDto
        {
            Id = employee.Id,
            CompanyId = employee.CompanyId,
            Name = employee.Name,
            Role = employee.Role,
            IsActive = employee.IsActive,
            TotalDocuments = employee.Documents.Count
        };
    }

    private static StpDocumentFileDto MapDocument(MvcStpCompanyDocumentFile document)
    {
        return new StpDocumentFileDto
        {
            Id = document.Id,
            Name = document.Name,
            OriginalFileName = document.OriginalFileName,
            MimeType = document.MimeType,
            SizeInBytes = document.SizeInBytes,
            CreatedAt = document.CreatedAt
        };
    }

    private static StpDocumentFileDto MapDocument(MvcStpEmployeeDocumentFile document)
    {
        return new StpDocumentFileDto
        {
            Id = document.Id,
            Name = document.Name,
            OriginalFileName = document.OriginalFileName,
            MimeType = document.MimeType,
            SizeInBytes = document.SizeInBytes,
            CreatedAt = document.CreatedAt
        };
    }

    private static string? ValidateUpload(StpDocumentUploadDto request)
    {
        if (request.Content.Length == 0 || request.SizeInBytes <= 0)
        {
            return "Selecione um arquivo para envio.";
        }

        if (request.SizeInBytes > MaxUploadSizeBytes)
        {
            return "O arquivo excede o limite de 15 MB.";
        }

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
        {
            return "O nome original do arquivo e obrigatorio.";
        }

        return null;
    }

    private static string NormalizeDocumentName(string? name, string originalFileName)
    {
        var normalized = NormalizeRequired(name);
        return normalized ?? Path.GetFileNameWithoutExtension(originalFileName);
    }

    private static string NormalizeMimeType(string? mimeType)
    {
        var normalized = NormalizeRequired(mimeType);
        return normalized ?? "application/octet-stream";
    }

    private static string? NormalizeRequired(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return NormalizeRequired(value);
    }
}
