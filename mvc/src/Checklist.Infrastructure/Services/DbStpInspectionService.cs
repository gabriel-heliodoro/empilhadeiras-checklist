using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

internal class DbStpInspectionService : IStpInspectionService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DbStpInspectionService(
        AppDbContext dbContext,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<StpDashboardDto>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpDashboardDto>.Fail("Usuario sem setor vinculado.");
        }

        var recentChecklists = await _dbContext.StpAreaChecklists
            .AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.InspectionArea)
                .ThenInclude(x => x!.ResponsibleSupervisor)
            .Include(x => x.InspectedSector)
            .Include(x => x.Items)
            .Include(x => x.InspectorSupervisor)
            .Where(x => x.SectorId == sectorId)
            .OrderByDescending(x => x.CompletedAt)
            .Take(6)
            .ToListAsync(cancellationToken);

        return Result<StpDashboardDto>.Ok(new StpDashboardDto
        {
            AreaCount = await _dbContext.StpInspectionAreas.CountAsync(x => x.SectorId == sectorId, cancellationToken),
            ChecklistCount = await _dbContext.StpAreaChecklists.CountAsync(x => x.SectorId == sectorId, cancellationToken),
            CompanyCount = await _dbContext.StpCompanyDocuments.CountAsync(x => x.SectorId == sectorId, cancellationToken),
            EmployeeDocumentCount = await _dbContext.StpEmployeeDocumentFiles.CountAsync(x => x.Employee.Company.SectorId == sectorId, cancellationToken),
            RecentChecklists = recentChecklists.Select(MapChecklistListItem).ToList()
        });
    }

    public async Task<Result<IReadOnlyList<StpAreaSummaryDto>>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<IReadOnlyList<StpAreaSummaryDto>>.Fail("Usuario sem setor vinculado.");
        }

        var areas = await _dbContext.StpInspectionAreas
            .AsNoTracking()
            .Include(x => x.ResponsibleSupervisor)
            .Where(x => x.SectorId == sectorId)
            .OrderBy(x => x.Name)
            .Select(x => new StpAreaSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                ResponsibleSupervisorId = x.ResponsibleSupervisorId,
                ResponsibleSupervisorName = (x.ResponsibleSupervisor.Name + " " + x.ResponsibleSupervisor.LastName).Trim(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StpAreaSummaryDto>>.Ok(areas);
    }

    public async Task<Result<IReadOnlyList<StpResponsibleOptionDto>>> GetResponsibleOptionsAsync(CancellationToken cancellationToken = default)
    {
        var responsibles = await _dbContext.SupervisorUsers
            .AsNoTracking()
            .Where(x => !x.IsMaster && x.IsActive && x.UserType == MvcUserAccessType.Supervisor)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.LastName)
            .Select(x => new StpResponsibleOptionDto
            {
                Id = x.Id,
                DisplayName = (x.Name + " " + x.LastName).Trim()
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StpResponsibleOptionDto>>.Ok(responsibles);
    }

    public async Task<Result> SaveAreaAsync(StpAreaUpsertDto request, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result.Fail("Usuario sem setor vinculado.");
        }

        var normalizedName = NormalizeRequired(request.Name);
        if (normalizedName is null)
        {
            return Result.Fail("Informe o nome da area.");
        }

        var responsibleExists = await _dbContext.SupervisorUsers.AnyAsync(
            x => x.Id == request.ResponsibleSupervisorId
                && !x.IsMaster
                && x.IsActive
                && x.UserType == MvcUserAccessType.Supervisor,
            cancellationToken);

        if (!responsibleExists)
        {
            return Result.Fail("Supervisor responsavel invalido ou inativo.");
        }

        var duplicated = await _dbContext.StpInspectionAreas.AnyAsync(
            x => x.SectorId == sectorId
                && x.Name == normalizedName
                && (!request.Id.HasValue || x.Id != request.Id.Value),
            cancellationToken);

        if (duplicated)
        {
            return Result.Fail("Ja existe uma area com este nome no setor.");
        }

        if (request.Id.HasValue)
        {
            var existing = await _dbContext.StpInspectionAreas.FirstOrDefaultAsync(
                x => x.Id == request.Id.Value && x.SectorId == sectorId,
                cancellationToken);

            if (existing is null)
            {
                return Result.Fail("Area STP nao encontrada.");
            }

            existing.Name = normalizedName;
            existing.ResponsibleSupervisorId = request.ResponsibleSupervisorId;
            existing.IsActive = request.IsActive;
        }
        else
        {
            _dbContext.StpInspectionAreas.Add(new MvcStpInspectionArea
            {
                SectorId = sectorId,
                Name = normalizedName,
                ResponsibleSupervisorId = request.ResponsibleSupervisorId,
                IsActive = request.IsActive
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<StpChecklistDraftDto>> GetChecklistDraftAsync(
        Guid? areaId,
        Guid? templateId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpChecklistDraftDto>.Fail("Usuario sem setor vinculado.");
        }

        var areas = await _dbContext.StpInspectionAreas
            .AsNoTracking()
            .Include(x => x.ResponsibleSupervisor)
            .Where(x => x.SectorId == sectorId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new StpAreaSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                ResponsibleSupervisorId = x.ResponsibleSupervisorId,
                ResponsibleSupervisorName = (x.ResponsibleSupervisor.Name + " " + x.ResponsibleSupervisor.LastName).Trim(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var templates = await _dbContext.StpAreaChecklistTemplates
            .AsNoTracking()
            .Where(x => x.SectorId == sectorId && x.IsActive)
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Name)
            .Select(x => new StpTemplateSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                IsActive = x.IsActive,
                ItemCount = x.Items.Count(i => i.IsActive)
            })
            .ToListAsync(cancellationToken);

        var selectedArea = areaId.HasValue
            ? areas.FirstOrDefault(x => x.Id == areaId.Value)
            : areas.FirstOrDefault();

        var effectiveTemplateId = templateId.HasValue && templates.Any(x => x.Id == templateId.Value)
            ? templateId
            : templates.FirstOrDefault()?.Id;

        StpTemplateDetailDto? selectedTemplate = null;
        if (effectiveTemplateId.HasValue)
        {
            selectedTemplate = await _dbContext.StpAreaChecklistTemplates
                .AsNoTracking()
                .Where(x => x.Id == effectiveTemplateId.Value && x.SectorId == sectorId && x.IsActive)
                .Select(x => new StpTemplateDetailDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Items = x.Items
                        .Where(i => i.IsActive)
                        .OrderBy(i => i.Order)
                        .Select(i => new StpTemplateItemDto
                        {
                            Id = i.Id,
                            Order = i.Order,
                            Description = i.Description,
                            Instruction = i.Instruction
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Result<StpChecklistDraftDto>.Ok(new StpChecklistDraftDto
        {
            SelectedAreaId = selectedArea?.Id,
            SelectedTemplateId = selectedTemplate?.Id,
            InspectorName = _currentUser.UserName ?? string.Empty,
            Areas = areas,
            Templates = templates,
            SelectedArea = selectedArea,
            SelectedTemplate = selectedTemplate
        });
    }

    public async Task<Result<StpChecklistResultDto>> SubmitChecklistAsync(
        StpChecklistSubmissionDto request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId) || !_currentUser.Id.HasValue)
        {
            return Result<StpChecklistResultDto>.Fail("Usuario autenticado invalido.");
        }

        if (request.AreaId == Guid.Empty)
        {
            return Result<StpChecklistResultDto>.Fail("Selecione uma area de inspecao.");
        }

        if (request.TemplateId == Guid.Empty)
        {
            return Result<StpChecklistResultDto>.Fail("Selecione um template STP.");
        }

        if (request.Items.Count == 0)
        {
            return Result<StpChecklistResultDto>.Fail("A inspecao precisa conter itens preenchidos.");
        }

        var area = await _dbContext.StpInspectionAreas
            .AsNoTracking()
            .Include(x => x.ResponsibleSupervisor)
            .FirstOrDefaultAsync(x => x.Id == request.AreaId && x.SectorId == sectorId && x.IsActive, cancellationToken);

        if (area is null)
        {
            return Result<StpChecklistResultDto>.Fail("A area informada nao existe ou esta inativa.");
        }

        var template = await _dbContext.StpAreaChecklistTemplates
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId && x.SectorId == sectorId && x.IsActive, cancellationToken);

        if (template is null)
        {
            return Result<StpChecklistResultDto>.Fail("O template STP informado nao existe ou esta inativo.");
        }

        var templateItems = template.Items
            .Where(x => x.IsActive)
            .OrderBy(x => x.Order)
            .ToList();

        if (templateItems.Count == 0)
        {
            return Result<StpChecklistResultDto>.Fail("O template STP nao possui itens ativos.");
        }

        var requestItemsById = new Dictionary<Guid, StpChecklistSubmissionItemDto>();
        foreach (var item in request.Items)
        {
            if (item.TemplateItemId == Guid.Empty)
            {
                return Result<StpChecklistResultDto>.Fail("Um ou mais itens do checklist estao invalidos.");
            }

            if (!requestItemsById.TryAdd(item.TemplateItemId, item))
            {
                return Result<StpChecklistResultDto>.Fail("Existem itens duplicados no envio da inspecao.");
            }
        }

        if (requestItemsById.Count != templateItems.Count || templateItems.Any(x => !requestItemsById.ContainsKey(x.Id)))
        {
            return Result<StpChecklistResultDto>.Fail("Os itens enviados nao correspondem ao template STP ativo.");
        }

        foreach (var templateItem in templateItems)
        {
            var requestItem = requestItemsById[templateItem.Id];
            var normalizedResult = NormalizeRequired(requestItem.Result);
            if (!string.Equals(normalizedResult, "Check", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(normalizedResult, "X", StringComparison.OrdinalIgnoreCase))
            {
                return Result<StpChecklistResultDto>.Fail($"O item {templateItem.Order} precisa ser marcado como Check ou X.");
            }

            if (string.Equals(normalizedResult, "X", StringComparison.OrdinalIgnoreCase)
                && NormalizeRequired(requestItem.Notes) is null)
            {
                return Result<StpChecklistResultDto>.Fail($"O item {templateItem.Order} exige observacao quando marcado com X.");
            }
        }

        var now = _dateTimeProvider.CurrentUtcDateTime;
        var checklist = new MvcStpAreaChecklist
        {
            SectorId = sectorId,
            InspectedSectorId = area.SectorId,
            InspectionAreaId = area.Id,
            TemplateId = template.Id,
            InspectorSupervisorId = _currentUser.Id.Value,
            PresentResponsibleName = (area.ResponsibleSupervisor.Name + " " + area.ResponsibleSupervisor.LastName).Trim(),
            PresentResponsibleRole = "Responsible supervisor",
            ObservedPreventiveBehaviors = NormalizeOptional(request.ObservedPreventiveBehaviors),
            ObservedUnsafeActs = NormalizeOptional(request.ObservedUnsafeActs),
            VerifiedUnsafeConditions = NormalizeOptional(request.VerifiedUnsafeConditions),
            InspectorSignatureBase64 = string.Empty,
            PresentResponsibleSignatureBase64 = string.Empty,
            InspectorSignedAt = now,
            PresentResponsibleSignedAt = now,
            CompletedAt = now,
            ReferenceDate = BusinessDate.TodayKeyUtc(),
            CreatedAt = now
        };

        foreach (var templateItem in templateItems)
        {
            var requestItem = requestItemsById[templateItem.Id];
            checklist.Items.Add(new MvcStpAreaChecklistItem
            {
                TemplateItemId = templateItem.Id,
                Order = templateItem.Order,
                Description = templateItem.Description,
                Instruction = templateItem.Instruction,
                Result = ParseResult(requestItem.Result),
                Notes = NormalizeOptional(requestItem.Notes),
                CreatedAt = now
            });
        }

        _dbContext.StpAreaChecklists.Add(checklist);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<StpChecklistResultDto>.Ok(new StpChecklistResultDto
        {
            Id = checklist.Id,
            CompletedAt = checklist.CompletedAt,
            InspectionAreaName = area.Name,
            TemplateCode = template.Code,
            TemplateName = template.Name
        });
    }

    public async Task<Result<IReadOnlyList<StpChecklistListItemDto>>> GetChecklistsAsync(
        StpChecklistListFiltersDto filters,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<IReadOnlyList<StpChecklistListItemDto>>.Fail("Usuario sem setor vinculado.");
        }

        var query = _dbContext.StpAreaChecklists
            .AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.InspectorSupervisor)
            .Include(x => x.InspectionArea)
                .ThenInclude(x => x!.ResponsibleSupervisor)
            .Include(x => x.InspectedSector)
            .Include(x => x.Items)
            .Where(x => x.SectorId == sectorId)
            .AsQueryable();

        if (filters.StartDate.HasValue)
        {
            var start = new DateTime(filters.StartDate.Value.Year, filters.StartDate.Value.Month, filters.StartDate.Value.Day, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(x => x.ReferenceDate >= start);
        }

        if (filters.EndDate.HasValue)
        {
            var end = new DateTime(filters.EndDate.Value.Year, filters.EndDate.Value.Month, filters.EndDate.Value.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
            query = query.Where(x => x.ReferenceDate < end);
        }

        if (!string.IsNullOrWhiteSpace(filters.Responsible))
        {
            var responsible = filters.Responsible.Trim().ToLowerInvariant();
            query = query.Where(x => x.PresentResponsibleName.ToLower().Contains(responsible));
        }

        var checklists = await query
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StpChecklistListItemDto>>.Ok(checklists.Select(MapChecklistListItem).ToList());
    }

    public async Task<Result<StpChecklistDetailsDto>> GetChecklistDetailsAsync(Guid checklistId, CancellationToken cancellationToken = default)
    {
        if (!TryGetSectorId(out var sectorId))
        {
            return Result<StpChecklistDetailsDto>.Fail("Usuario sem setor vinculado.");
        }

        var checklist = await _dbContext.StpAreaChecklists
            .AsNoTracking()
            .Include(x => x.Template)
            .Include(x => x.InspectorSupervisor)
            .Include(x => x.InspectionArea)
                .ThenInclude(x => x!.ResponsibleSupervisor)
            .Include(x => x.InspectedSector)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == checklistId && x.SectorId == sectorId, cancellationToken);

        if (checklist is null)
        {
            return Result<StpChecklistDetailsDto>.Fail("Checklist STP nao encontrado.");
        }

        return Result<StpChecklistDetailsDto>.Ok(new StpChecklistDetailsDto
        {
            Id = checklist.Id,
            InspectionAreaName = checklist.InspectionArea?.Name ?? checklist.InspectedSector.Name,
            TemplateCode = checklist.Template.Code,
            TemplateName = checklist.Template.Name,
            CompletedAt = checklist.CompletedAt,
            ReferenceDate = checklist.ReferenceDate,
            InspectorName = (checklist.InspectorSupervisor.Name + " " + checklist.InspectorSupervisor.LastName).Trim(),
            ResponsibleName = checklist.InspectionArea is not null
                ? (checklist.InspectionArea.ResponsibleSupervisor.Name + " " + checklist.InspectionArea.ResponsibleSupervisor.LastName).Trim()
                : checklist.PresentResponsibleName,
            ObservedPreventiveBehaviors = checklist.ObservedPreventiveBehaviors,
            ObservedUnsafeActs = checklist.ObservedUnsafeActs,
            VerifiedUnsafeConditions = checklist.VerifiedUnsafeConditions,
            Items = checklist.Items
                .OrderBy(x => x.Order)
                .Select(x => new StpChecklistItemDto
                {
                    Order = x.Order,
                    Description = x.Description,
                    Instruction = x.Instruction,
                    Result = x.Result == MvcStpAreaChecklistResult.Check ? "Check" : "X",
                    Notes = x.Notes
                })
                .ToList()
        });
    }

    private bool TryGetSectorId(out Guid sectorId)
    {
        sectorId = _currentUser.SectorId ?? Guid.Empty;
        return sectorId != Guid.Empty;
    }

    private static StpChecklistListItemDto MapChecklistListItem(MvcStpAreaChecklist checklist)
    {
        return new StpChecklistListItemDto
        {
            Id = checklist.Id,
            TemplateId = checklist.TemplateId,
            TemplateCode = checklist.Template.Code,
            TemplateName = checklist.Template.Name,
            CompletedAt = checklist.CompletedAt,
            InspectorName = (checklist.InspectorSupervisor.Name + " " + checklist.InspectorSupervisor.LastName).Trim(),
            InspectionAreaName = checklist.InspectionArea?.Name ?? checklist.InspectedSector.Name,
            ResponsibleName = checklist.InspectionArea is not null
                ? (checklist.InspectionArea.ResponsibleSupervisor.Name + " " + checklist.InspectionArea.ResponsibleSupervisor.LastName).Trim()
                : checklist.PresentResponsibleName,
            TotalItems = checklist.Items.Count,
            TotalCheck = checklist.Items.Count(x => x.Result == MvcStpAreaChecklistResult.Check),
            TotalX = checklist.Items.Count(x => x.Result == MvcStpAreaChecklistResult.X)
        };
    }

    private static MvcStpAreaChecklistResult ParseResult(string? value)
    {
        return string.Equals(value, "X", StringComparison.OrdinalIgnoreCase)
            ? MvcStpAreaChecklistResult.X
            : MvcStpAreaChecklistResult.Check;
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
