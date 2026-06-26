using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

internal class DbOperatorChecklistService : IOperatorChecklistService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentOperator _currentOperator;

    public DbOperatorChecklistService(AppDbContext dbContext, ICurrentOperator currentOperator)
    {
        _dbContext = dbContext;
        _currentOperator = currentOperator;
    }

    public async Task<Result<OperatorChecklistDraftDto>> GetDraftAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.Id.HasValue || !_currentOperator.SectorId.HasValue)
        {
            return Result<OperatorChecklistDraftDto>.Fail("Operator autenticado invalido.");
        }

        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == equipmentId && x.IsActive && x.SectorId == _currentOperator.SectorId.Value, cancellationToken);

        if (equipment is null)
        {
            return Result<OperatorChecklistDraftDto>.Fail("Equipment nao encontrado ou inativo.");
        }

        var operatorData = await _dbContext.Operators
            .AsNoTracking()
            .Include(x => x.Sector)
            .FirstOrDefaultAsync(x => x.Id == _currentOperator.Id.Value && x.IsActive, cancellationToken);

        if (operatorData is null || !operatorData.Sector.IsActive)
        {
            return Result<OperatorChecklistDraftDto>.Fail("Operator nao encontrado ou inativo.");
        }

        var templates = await _dbContext.ChecklistItemTemplates
            .AsNoTracking()
            .Where(x => x.CategoryId == equipment.CategoryId && x.SectorId == equipment.SectorId && x.IsActive)
            .OrderBy(x => x.Order)
            .Select(x => new OperatorChecklistTemplateItemDto
            {
                Id = x.Id,
                SectorId = x.SectorId,
                CategoryId = x.CategoryId,
                Order = x.Order,
                Description = x.Description,
                Instruction = x.Instruction,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            return Result<OperatorChecklistDraftDto>.Fail("Nao ha itens de checklist configurados para esta categoria.");
        }

        return Result<OperatorChecklistDraftDto>.Ok(new OperatorChecklistDraftDto
        {
            Equipment = new OperatorEquipmentDto
            {
                Id = equipment.Id,
                SectorId = equipment.SectorId,
                CategoryId = equipment.CategoryId,
                QrId = equipment.QrId,
                Code = equipment.Code,
                Description = equipment.Description,
                CategoryName = equipment.Category.Name,
                IsActive = equipment.IsActive
            },
            Operator = new OperatorSessionDto
            {
                Id = operatorData.Id,
                SectorId = operatorData.SectorId,
                Name = $"{operatorData.Name} {operatorData.LastName}".Trim(),
                Registration = operatorData.Registration,
                Login = operatorData.Login,
                SectorName = operatorData.Sector.Name,
                ForceChangePassword = operatorData.ForceChangePassword
            },
            ItemsTemplate = templates
        });
    }

    public async Task<Result<OperatorChecklistResultDto>> SubmitAsync(
        OperatorChecklistSubmissionDto request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.Id.HasValue || !_currentOperator.SectorId.HasValue)
        {
            return Result<OperatorChecklistResultDto>.Fail("Operator autenticado invalido.");
        }

        if (_currentOperator.ForceChangePassword)
        {
            return Result<OperatorChecklistResultDto>.Fail("O operador precisa atualizar a senha antes de enviar o checklist.");
        }

        if (request.EquipmentId == Guid.Empty)
        {
            return Result<OperatorChecklistResultDto>.Fail("EquipmentId e obrigatorio.");
        }

        if (request.Items.Count == 0)
        {
            return Result<OperatorChecklistResultDto>.Fail("Checklist deve ter pelo menos um item.");
        }

        if (string.IsNullOrWhiteSpace(request.OperatorSignatureBase64))
        {
            return Result<OperatorChecklistResultDto>.Fail("A assinatura do operador e obrigatoria.");
        }

        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == request.EquipmentId && x.IsActive, cancellationToken);

        if (equipment is null)
        {
            return Result<OperatorChecklistResultDto>.Fail("Equipment invalido ou inativo.");
        }

        var operatorData = await _dbContext.Operators
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == _currentOperator.Id.Value && x.IsActive, cancellationToken);

        if (operatorData is null)
        {
            return Result<OperatorChecklistResultDto>.Fail("Operator invalido ou inativo.");
        }

        if (request.OperatorId != Guid.Empty && request.OperatorId != operatorData.Id)
        {
            return Result<OperatorChecklistResultDto>.Fail("O operador autenticado nao corresponde ao operador informado.");
        }

        if (operatorData.SectorId != equipment.SectorId || operatorData.SectorId != _currentOperator.SectorId.Value)
        {
            return Result<OperatorChecklistResultDto>.Fail("Operator e equipamento precisam pertencer ao mesmo setor.");
        }

        if (equipment.Category.SectorId != equipment.SectorId)
        {
            return Result<OperatorChecklistResultDto>.Fail("O equipamento esta vinculado a uma categoria de outro setor.");
        }

        var templates = await _dbContext.ChecklistItemTemplates
            .AsNoTracking()
            .Where(x => x.CategoryId == equipment.CategoryId && x.SectorId == equipment.SectorId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            return Result<OperatorChecklistResultDto>.Fail("Nao ha itens de checklist configurados para esta categoria.");
        }

        var templateIds = templates.Select(x => x.Id).ToHashSet();
        if (request.Items.Any(x => !templateIds.Contains(x.TemplateId)))
        {
            return Result<OperatorChecklistResultDto>.Fail("Um ou mais itens nao correspondem aos templates da categoria.");
        }

        var itemNokSemNotes = request.Items.FirstOrDefault(x =>
            string.Equals(x.Status, "NOK", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(x.Notes));

        if (itemNokSemNotes is not null)
        {
            return Result<OperatorChecklistResultDto>.Fail("Items marcados como NOK exigem observacao obrigatoria.");
        }

        var itemComImagemInvalida = request.Items.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.NokImageBase64)
            && !IsValidChecklistImagePayload(x.NokImageBase64!, x.NokImageMimeType));

        if (itemComImagemInvalida is not null)
        {
            return Result<OperatorChecklistResultDto>.Fail("A imagem anexada em um item NOK esta invalida ou excede o limite suportado.");
        }

        var dataReferencia = BusinessDate.TodayKeyUtc();
        var proximaReferenceDate = dataReferencia.AddDays(1);

        var alreadyExists = await _dbContext.Checklists
            .AsNoTracking()
            .AnyAsync(x =>
                x.SectorId == equipment.SectorId
                && x.EquipmentId == request.EquipmentId
                && x.ReferenceDate >= dataReferencia
                && x.ReferenceDate < proximaReferenceDate,
                cancellationToken);

        if (alreadyExists)
        {
            return Result<OperatorChecklistResultDto>.Fail("Este equipamento ja possui checklist registrado hoje.");
        }

        var checklist = new MvcChecklist
        {
            Id = Guid.NewGuid(),
            SectorId = equipment.SectorId,
            EquipmentId = request.EquipmentId,
            OperatorId = operatorData.Id,
            ReferenceDate = dataReferencia,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            GeneralNotes = NormalizeOptionalText(request.GeneralNotes),
            OperatorSignatureBase64 = request.OperatorSignatureBase64.Trim(),
            SignedAt = DateTime.UtcNow,
            Status = MvcChecklistStatus.Pending
        };

        foreach (var requestItem in request.Items)
        {
            var template = templates.First(x => x.Id == requestItem.TemplateId);
            var mappedStatus = ParseItemStatus(requestItem.Status);
            if (!mappedStatus.HasValue || mappedStatus.Value == MvcItemStatus.NotChecked)
            {
                return Result<OperatorChecklistResultDto>.Fail("Todos os itens devem ser respondidos antes do envio.");
            }

            checklist.Items.Add(new MvcChecklistItem
            {
                Id = Guid.NewGuid(),
                TemplateId = requestItem.TemplateId,
                CreatedAt = DateTime.UtcNow,
                Order = template.Order,
                Description = template.Description,
                Instruction = template.Instruction,
                Status = mappedStatus.Value,
                Notes = mappedStatus.Value == MvcItemStatus.NOK ? NormalizeOptionalText(requestItem.Notes) : null,
                NokImageBase64 = mappedStatus.Value == MvcItemStatus.NOK ? NormalizeOptionalBase64(requestItem.NokImageBase64) : null,
                NokImageFileName = mappedStatus.Value == MvcItemStatus.NOK ? NormalizeOptionalText(requestItem.NokImageFileName) : null,
                NokImageMimeType = mappedStatus.Value == MvcItemStatus.NOK ? NormalizeOptionalText(requestItem.NokImageMimeType) : null
            });
        }

        checklist.IsApproved = checklist.Items.All(x => x.Status is MvcItemStatus.OK or MvcItemStatus.NA);
        if (!checklist.IsApproved)
        {
            checklist.Status = MvcChecklistStatus.Rejected;
        }

        _dbContext.Checklists.Add(checklist);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<OperatorChecklistResultDto>.Ok(new OperatorChecklistResultDto
        {
            Id = checklist.Id,
            SectorId = checklist.SectorId,
            EquipmentId = checklist.EquipmentId,
            EquipmentCode = equipment.Code,
            OperatorId = operatorData.Id,
            OperatorName = $"{operatorData.Name} {operatorData.LastName}".Trim(),
            CompletedAtUtc = checklist.CompletedAt,
            IsApproved = checklist.IsApproved,
            Status = checklist.Status.ToString()
        });
    }

    private static MvcItemStatus? ParseItemStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<MvcItemStatus>(value, true, out var parsedStatus)
            ? parsedStatus
            : null;
    }

    private static bool IsValidChecklistImagePayload(string imageBase64, string? mimeType)
    {
        var normalizedImage = NormalizeOptionalBase64(imageBase64);
        var normalizedMimeType = NormalizeOptionalText(mimeType);

        if (string.IsNullOrWhiteSpace(normalizedImage))
        {
            return false;
        }

        if (!normalizedImage.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalizedMimeType is not null && !normalizedMimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return normalizedImage.Length <= 8_000_000;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeOptionalBase64(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
