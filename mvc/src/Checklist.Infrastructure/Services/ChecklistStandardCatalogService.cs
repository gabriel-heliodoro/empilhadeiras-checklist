using Checklist.Infrastructure.Common;
using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class ChecklistStandardCatalogService
{
    private readonly AppDbContext _dbContext;

    public ChecklistStandardCatalogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureDefaultsForSectorAsync(Guid sectorId, CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.EquipmentCategories
            .Include(category => category.ChecklistItemTemplates)
            .Where(category => category.SectorId == sectorId)
            .ToListAsync(cancellationToken);

        foreach (var definition in Definitions)
        {
            var category = categories.FirstOrDefault(x => x.MonthlyClosureModel == definition.Model)
                ?? categories.FirstOrDefault(x => string.Equals(x.Name, definition.Name, StringComparison.OrdinalIgnoreCase));

            if (category is null)
            {
                category = new MvcEquipmentCategory
                {
                    SectorId = sectorId,
                    Name = definition.Name,
                    IsActive = true,
                    MonthlyClosureModel = definition.Model
                };

                _dbContext.EquipmentCategories.Add(category);
                categories.Add(category);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException exception) when (DatabaseErrorDetector.IsDuplicateKey(exception))
                {
                    _dbContext.Entry(category).State = EntityState.Detached;
                    categories.Remove(category);

                    category = await _dbContext.EquipmentCategories
                        .Include(x => x.ChecklistItemTemplates)
                        .FirstOrDefaultAsync(
                            x => x.SectorId == sectorId && x.Name == definition.Name,
                            cancellationToken);

                    if (category is null)
                    {
                        throw;
                    }

                    categories.Add(category);
                }
            }
            else if (category.MonthlyClosureModel == MvcMonthlyClosureModel.None)
            {
                category.MonthlyClosureModel = definition.Model;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var existingOrders = category.ChecklistItemTemplates
                .Select(item => item.Order)
                .ToHashSet();

            foreach (var itemDefinition in definition.Items.Where(item => !existingOrders.Contains(item.Order)))
            {
                var item = new MvcChecklistItemTemplate
                {
                    SectorId = sectorId,
                    CategoryId = category.Id,
                    Order = itemDefinition.Order,
                    Description = itemDefinition.Description,
                    IsActive = true
                };

                category.ChecklistItemTemplates.Add(item);
                _dbContext.ChecklistItemTemplates.Add(item);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly StandardCategoryDefinition[] Definitions =
    [
        new(
            "Empilhadeira a Combustao",
            MvcMonthlyClosureModel.CombustionForklift,
            [
                new(1, "Pneus (dianteiro e traseiro) estao em bom estado?"),
                new(2, "Equipamento esta sem vazamento?"),
                new(3, "Comandos hidraulicos da maquina estao funcionando?"),
                new(4, "Nivel de oleo do motor esta adequado?"),
                new(5, "Nivel da agua do radiador esta adequado?"),
                new(6, "Nivel de Oleo Hidraulico esta adequado?"),
                new(7, "Direcao encontra-se em condicoes de uso?"),
                new(8, "Freio esta regulado?"),
                new(9, "Extintor encontra-se fixo no equipamento ?"),
                new(10, "Farois estao funcionando?"),
                new(11, "Sistema sonoro de re esta funcionando?"),
                new(12, "Buzina em perfeito estado de funcionamento?"),
                new(13, "Garfos estao em bom estado?")
            ]),
        new(
            "Empilhadeira Eletrica",
            MvcMonthlyClosureModel.ElectricForklift,
            [
                new(1, "Rodas (dianteira e traseira) estao em bom estado?"),
                new(2, "Bateria esta funcionando normalmente?"),
                new(3, "Existe oxidacao na bateria?"),
                new(4, "Nivel de oleo do motor esta adequado?"),
                new(5, "Nivel da agua do radiador esta adequado?"),
                new(6, "Nivel de Oleo Hidraulico esta adequado?"),
                new(7, "Direcao encontra-se em condicoes de uso?"),
                new(8, "Freio esta regulado?"),
                new(9, "Extintor encontra-se fixo no equipamento ?"),
                new(10, "Farois estao funcionando?"),
                new(11, "Sistema sonoro de re esta funcionando?"),
                new(12, "Buzina em perfeito estado de funcionamento?"),
                new(13, "Garfos estao em bom estado?")
            ])
    ];

    private sealed record StandardCategoryDefinition(
        string Name,
        MvcMonthlyClosureModel Model,
        IReadOnlyList<StandardChecklistItemDefinition> Items);

    private sealed record StandardChecklistItemDefinition(
        int Order,
        string Description);
}
