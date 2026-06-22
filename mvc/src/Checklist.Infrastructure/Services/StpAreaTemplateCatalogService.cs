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

        var existingOrders = existingTemplate.Items.Select(item => item.Order).ToHashSet();

        foreach (var itemDefinition in DefaultTemplate.Items.Where(item => !existingOrders.Contains(item.Order)))
        {
            _dbContext.StpAreaChecklistTemplateItems.Add(new MvcStpAreaChecklistTemplateItem
            {
                TemplateId = existingTemplate.Id,
                Order = itemDefinition.Order,
                Description = itemDefinition.Description,
                Instruction = itemDefinition.Instruction,
                IsActive = true
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static readonly StpAreaTemplateDefinition DefaultTemplate = new(
        "F_PTV_0232",
        "Inspecao de Campo SMS",
        [
            new(1, "Os colaboradores conhecem e entendem sobre o tema do DSS realizado na semana?", "Realizar entrevista amostral com colaboradores do setor sobre o DSS da semana e registrar eventual desvio."),
            new(2, "Os check-lists de seguranca estao preenchidos de forma correta", "Verificar os check-lists de seguranca da area e analisar criticamente o preenchimento."),
            new(3, "Os dispositivos de emergencia foram verificados antes do uso de acordo com seus respectivos registros", "Verificar testes e registros dos dispositivos de emergencia antes do uso."),
            new(4, "Os mobiliarios disponiveis estao em boas condicoes", "Verificar se os mobiliarios do ambiente estao integros e funcionais."),
            new(5, "Os sistemas de prevencao contra incendio e emergencia estao desobstruidos", "Verificar extintores, hidrantes, luzes de emergencia, armarios corta-fogo, chuveiros e demais recursos."),
            new(6, "As sinalizacoes de seguranca estao adequadas", "Verificar rotas de fuga, ponto de encontro, mapas de risco, uso de EPIs e demais sinalizacoes."),
            new(7, "Os dispositivos eletricos estao em boas condicoes de uso, sinalizados e identificados de maneira correta", "Verificar tomadas, cabos, fios e demais dispositivos eletricos quanto a integridade, sinalizacao e identificacao."),
            new(8, "Os paineis e quadros eletricos estao com as portas fechadas, com impedimento de acesso acidental", "Verificar paineis e quadros fechados, identificados e desobstruidos."),
            new(9, "Todas as luminarias instaladas no local estao em funcionamento", "Verificar se ha lampadas apagadas, queimadas ou danificadas no ambiente."),
            new(10, "A organizacao e limpeza do local encontram-se em boas condicoes", "Verificar organizacao e limpeza do ambiente considerando os principios de 5S."),
            new(11, "Os produtos quimicos estao identificados e acondicionados de forma correta", "Verificar identificacao, acondicionamento, fracionamento, contencao e documentacao GHS dos produtos."),
            new(12, "As escadas portateis estao em boas condicoes para uso", "Verificar condicoes das escadas, lacres de inspecao e conhecimento de uso pelos colaboradores."),
            new(13, "As maquinas e equipamentos estao com suas protecoes integras e em conformidade com a NR-12", "Verificar protecoes dos equipamentos, maquinarios e a consistencia dos check-lists da NR-12."),
            new(14, "As ferramentas utilizadas estao em boas condicoes de uso", "Verificar integridade das ferramentas e uso adequado para a finalidade destinada."),
            new(15, "Os equipamentos e acessorios de movimentacao de carga estao disponiveis e em condicoes de uso, com seus respectivos check-lists antes do uso preenchidos de forma correta", "Verificar equipamentos e acessorios de movimentacao de carga, integridade, identificacao de inspecoes e check-lists antes do uso."),
            new(16, "As areas de movimentacao de materiais e pessoas estao demarcadas", "Verificar se as areas de movimentacao de pessoas e materiais estao devidamente demarcadas e sinalizadas."),
            new(17, "Os colaboradores conhecem e utilizam os EPIs para a finalidade em que e destinada", "Realizar verificacao amostral do uso correto de EPIs e do conhecimento de sua aplicacao."),
            new(18, "Os colaboradores conhecem o PAE (Plano de Atendimento a Emergencia)", "Realizar entrevista amostral sobre telefone de emergencia, ponto de encontro, rotas de fuga e cenarios de emergencia."),
            new(19, "Os colaboradores estao portando seus crachas de autorizacao e estao validos", "Verificar crachas e validade dos treinamentos e autorizacoes associadas."),
            new(20, "Os colaboradores conhecem o formulario Relato de Anomalias de SMS", "Realizar entrevista amostral sobre aplicacao, busca e registro do formulario de anomalias."),
            new(21, "Os colaboradores conhecem a LAAIPD", "Realizar entrevista amostral sobre riscos das atividades e localizacao das informacoes da LAAIPD."),
            new(22, "Os colaboradores conhecem e sabem como contribuir com a Politica de SMS", "Realizar entrevista amostral sobre a politica de SMS e como contribuir com ela no dia a dia.")
        ]);

    private sealed record StpAreaTemplateDefinition(
        string Code,
        string Name,
        IReadOnlyList<StpAreaTemplateItemDefinition> Items);

    private sealed record StpAreaTemplateItemDefinition(
        int Order,
        string Description,
        string Instruction);
}
