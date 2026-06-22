using Checklist.Application.Common;
using Checklist.Application.Dtos;
using Checklist.Application.Interfaces;
using Checklist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

internal class DbOperatorEquipmentReader : IOperatorEquipmentReader
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentOperator _currentOperator;

    public DbOperatorEquipmentReader(AppDbContext dbContext, ICurrentOperator currentOperator)
    {
        _dbContext = dbContext;
        _currentOperator = currentOperator;
    }

    public async Task<Result<IReadOnlyList<OperatorEquipmentDto>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.SectorId.HasValue)
        {
            return Result<IReadOnlyList<OperatorEquipmentDto>>.Fail("Sector do operador atual nao foi resolvido.");
        }

        var normalizedQuery = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return Result<IReadOnlyList<OperatorEquipmentDto>>.Fail("Informe um codigo ou QR ID para buscar o equipamento.");
        }

        var equipmentQuery = _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsActive && x.SectorId == _currentOperator.SectorId.Value);

        if (Guid.TryParse(normalizedQuery, out var qrId))
        {
            equipmentQuery = equipmentQuery.Where(x => x.QrId == qrId);
        }
        else
        {
            var normalizedCode = normalizedQuery.ToUpperInvariant();
            equipmentQuery = equipmentQuery.Where(x =>
                x.Code == normalizedCode
                || x.Code.StartsWith(normalizedCode)
                || x.Description.Contains(normalizedQuery));
        }

        var results = await equipmentQuery
            .OrderBy(x => x.Code)
            .Select(MapEquipment())
            .Take(12)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OperatorEquipmentDto>>.Ok(results);
    }

    public async Task<Result<OperatorEquipmentDto>> GetByIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.SectorId.HasValue)
        {
            return Result<OperatorEquipmentDto>.Fail("Sector do operador atual nao foi resolvido.");
        }

        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Id == equipmentId && x.IsActive && x.SectorId == _currentOperator.SectorId.Value)
            .Select(MapEquipment())
            .FirstOrDefaultAsync(cancellationToken);

        if (equipment is null)
        {
            return Result<OperatorEquipmentDto>.Fail("Equipment nao encontrado ou inativo.");
        }

        return Result<OperatorEquipmentDto>.Ok(equipment);
    }

    public async Task<Result<OperatorEquipmentDto>> GetByQrIdAsync(Guid qrId, CancellationToken cancellationToken = default)
    {
        if (!_currentOperator.SectorId.HasValue)
        {
            return Result<OperatorEquipmentDto>.Fail("Sector do operador atual nao foi resolvido.");
        }

        var equipment = await _dbContext.Equipment
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.QrId == qrId && x.IsActive && x.SectorId == _currentOperator.SectorId.Value)
            .Select(MapEquipment())
            .FirstOrDefaultAsync(cancellationToken);

        if (equipment is null)
        {
            return Result<OperatorEquipmentDto>.Fail("Equipment nao encontrado para este QR ID.");
        }

        return Result<OperatorEquipmentDto>.Ok(equipment);
    }

    private static System.Linq.Expressions.Expression<Func<Data.Models.MvcEquipment, OperatorEquipmentDto>> MapEquipment()
    {
        return equipamento => new OperatorEquipmentDto
        {
            Id = equipamento.Id,
            SectorId = equipamento.SectorId,
            CategoryId = equipamento.CategoryId,
            QrId = equipamento.QrId,
            Code = equipamento.Code,
            Description = equipamento.Description,
            CategoryName = equipamento.Category.Name,
            IsActive = equipamento.IsActive
        };
    }
}
