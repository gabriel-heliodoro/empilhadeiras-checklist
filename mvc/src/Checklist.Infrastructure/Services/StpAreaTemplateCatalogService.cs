using Checklist.Infrastructure.Data;
using Checklist.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Checklist.Infrastructure.Services;

public class StpAreaTemplateCatalogService
{
    private readonly AppDbContext _dbContext;

    public StpAreaTemplateCatalogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureDefaultsForSectorAsync(Guid sectorId, CancellationToken cancellationToken = default)
    {
        var existingTemplate = await _dbContext.StpAreaChecklistTemplates
            .Include(template => template.Items)
            .FirstOrDefaultAsync(
                template => template.SectorId == sectorId && template.Code == DefaultTemplate.Code,
                cancellationToken);

        if (existingTemplate is null)
        {
            existingTemplate = new MvcStpAreaChecklistTemplate
            {
                SectorId = sectorId,
                Code = DefaultTemplate.Code,
                Name = DefaultTemplate.Name,
                IsActive = true
            };

            _dbContext.StpAreaChecklistTemplates.Add(existingTemplate);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        existingTemplate.Name = DefaultTemplate.Name;
        existingTemplate.IsActive = true;

        foreach (var itemDefinition in DefaultTemplate.Items)
        {
            var existingItem = existingTemplate.Items.FirstOrDefault(item => item.Order == itemDefinition.Order);
            if (existingItem is null)
            {
                _dbContext.StpAreaChecklistTemplateItems.Add(new MvcStpAreaChecklistTemplateItem
                {
                    TemplateId = existingTemplate.Id,
                    Order = itemDefinition.Order,
                    Description = itemDefinition.Description,
                    Instruction = itemDefinition.Instruction,
                    IsActive = true
                });

                continue;
            }

            existingItem.Description = itemDefinition.Description;
            existingItem.Instruction = itemDefinition.Instruction;
            existingItem.IsActive = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly StpAreaTemplateDefinition DefaultTemplate = new(
        "F_PTV_0232",
        "Inspecao de Campo SMS",
        [
            new(1, "Os colaboradores conhecem e entendem sobre o tema do DSS realizado na semana", null),
            new(2, "Os check-list de seguranca estao preenchidos de forma correta", null),
            new(3, "Os dispositivos de emergencia foram verificados antes do uso de acordo com seus respectivos registros", null),
            new(4, "Os mobiliarios disponiveis estao em boas condicoes", null),
            new(5, "Os sistemas de prevencao contra incendio e emergencia estao desobstruidos", null),
            new(6, "As sinalizacoes de seguranca estao adequadas", null),
            new(7, "Os dispositivos eletricos estao em boas condicoes de uso, sinalizados e identificados de maneira correta", null),
            new(8, "Os paineis e quadros eletricos estao com as portas fechadas, com impedimento de acesso acidental", null),
            new(9, "Todas as luminarias instaladas no local estao em funcionamento", null),
            new(10, "A organizacao e limpeza do local encontram-se em boas condicoes", null),
            new(11, "Os produtos quimicos estao identificados e acondicionados de forma correta", null),
            new(12, "As escadas portateis estao em boas condicoes para uso", null),
            new(13, "As maquinas e equipamentos rotativos estao com suas protecoes integras", null),
            new(14, "As ferramentas utilizadas estao em boas condicoes de uso", null),
            new(15, "Os equipamentos e acessorios de guindar estao disponiveis e em condicoes de uso, com seus respectivos check-lists antes do uso preenchidos de forma correta", null),
            new(16, "As areas de movimentacao de materiais e pessoas estao demarcadas", null),
            new(17, "Os colaboradores conhecem e utilizam os EPIs para a finalidade em que e destinada", null),
            new(18, "Os colaboradores conhecem o PAE (Plano de Atendimento a Emergencia)", null),
            new(19, "Os colaboradores estao portando seus crachas de autorizacao e estao validos", null),
            new(20, "Os colaboradores conhecem o formulario Relato de Anomalias de SMS", null),
            new(21, "Os colaboradores conhecem a LAAIPD (Levantamento e Avaliacao de Aspectos/Impactos e Perigos/Danos)", null),
            new(22, "Os colaboradores conhecem e sabem como contribuir com a Politica de SMS", null)
        ]);

    private sealed record StpAreaTemplateDefinition(
        string Code,
        string Name,
        IReadOnlyList<StpAreaTemplateItemDefinition> Items);

    private sealed record StpAreaTemplateItemDefinition(
        int Order,
        string Description,
        string? Instruction);
}
