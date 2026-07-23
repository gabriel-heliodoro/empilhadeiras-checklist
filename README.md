# CheckFlow

Versao atual: `1.0.0`

Sistema para checklist operacional de empilhadeiras, supervisao de nao conformidades, inspecoes STP e administracao de catalogos operacionais.

## Arquitetura atual

O projeto adota uma unica aplicacao ASP.NET Core MVC como ponto de entrada.

- UI, autenticacao e fluxo HTTP: `mvc/src/Checklist.Mvc`
- regras de aplicacao: `mvc/src/Checklist.Application`
- dominio: `mvc/src/Checklist.Domain`
- persistencia, identidade e integracoes: `mvc/src/Checklist.Infrastructure`
- banco principal: SQL Server

## Modulos

- Operacao: login do operador, leitura por QR ID, abertura e envio de checklist
- Supervisao: dashboard, historico, detalhe de checklist e painel de itens non-compliant
- Catalogos: categorias, templates, operadores e equipamentos
- Master: setores, supervisores e inspetores
- STP: dashboard, areas, checklists e documentos
- Fechamento mensal: consolidacao e exportacao

## Estrutura do repositorio

```text
empilhadeiras-checklist/
|-- mvc/
|   |-- src/
|   |   |-- Checklist.Application/
|   |   |-- Checklist.Domain/
|   |   |-- Checklist.Infrastructure/
|   |   `-- Checklist.Mvc/
|   `-- Checklist.Mvc.slnx
|-- infra/
|-- docs/
|-- CHANGELOG.md
|-- CONTRIBUTING.md
|-- DEPLOY.md
`-- README.md
```

## Configuracao

### Banco

O MVC usa connection string direta.

```

## Persistencia

- Provider principal: SQL Server via EF Core
- `AppDbContext`: `mvc/src/Checklist.Infrastructure/Data/AppDbContext.cs`

Perfis principais:

- `Master`
- `Supervisor`
- `Inspector`
- `Operator`

Politicas principais:

- `MasterReady`
- `SectorSupervisorReady`
- `SafetyWorkReady`
- `MaterialsInspectionReady`
- `OperatorAuthenticated`
- `OperatorChecklistReady`
