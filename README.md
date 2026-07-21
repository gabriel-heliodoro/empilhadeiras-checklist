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
- autenticacao administrativa: cookie + Active Directory em Windows
- autenticacao operacional: cookie + credenciais do operador persistidas no banco

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

Ordem de resolucao:

1. `ConnectionStrings__Default`
2. `ConnectionStrings:Default`
3. `ConnectionStrings:AppDbConnectionString`

Exemplo local:

```env
ConnectionStrings__Default=Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

### Autenticacao

Supervisor, inspetor e operador autenticam sempre via Active Directory, exceto a conta
`IsMaster` (admin de bootstrap), que usa senha local. Dominio e OU do AD sao fixos em
`Checklist.Infrastructure/Services/ActiveDirectoryService.cs` (nao vem de `appsettings`).

```json
{
  "Authentication": {
    "Mode": "DevelopmentStub"
  }
}
```

`Authentication:Mode` hoje so controla se o `LocalMvcDatabaseBootstrapper` semeia um
supervisor/operador de desenvolvimento no banco local (`DevelopmentStub`) — nao afeta mais
a validacao de login, que sempre bate no AD real.

### Credenciais locais

- conta master (bootstrap, senha local): configurada via `MasterController` apos o primeiro
  supervisor `IsMaster` existir no banco.
- supervisor, inspetor e operador: login precisa corresponder a uma conta valida no AD
  configurado em `ActiveDirectoryService`.

## Execucao local

### MVC com SQL Server local

Voce ja pode rodar sem Docker Compose.

Se quiser usar a connection string do `appsettings.Development.json`, basta:

```powershell
dotnet run --project mvc/src/Checklist.Mvc/Checklist.Mvc.csproj --urls http://localhost:5204
```

Se preferir sobrescrever por ambiente:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__Default="Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
dotnet run --project mvc/src/Checklist.Mvc/Checklist.Mvc.csproj --urls http://localhost:5204
```

### MVC sem banco configurado

Se nenhuma conexao for informada, a aplicacao usa banco em memoria para desenvolvimento local. Esse modo serve para navegacao e refinamento de UI, nao para validar persistencia real em SQL Server.

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project mvc/src/Checklist.Mvc/Checklist.Mvc.csproj --urls http://localhost:5204
```

## Build

```powershell
dotnet build Checklist.Mvc.slnx
```

No estado atual, ainda nao existe projeto de testes dedicado dentro de `mvc/tests`.

## Persistencia

- Provider principal: SQL Server via EF Core
- `AppDbContext`: `mvc/src/Checklist.Infrastructure/Data/AppDbContext.cs`
- Quando nao ha conexao configurada, o projeto cai para banco em memoria
- O bootstrap local usa `EnsureCreatedAsync` e seed minimo para o fluxo de desenvolvimento

No estado atual, a linha MVC ainda nao possui migrations versionadas equivalentes a um fluxo formal de producao. Enquanto isso, o schema local e criado pelo bootstrap da propria aplicacao.

## Perfis e politicas

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

## Documentacao complementar

- [CONTRIBUTING.md](CONTRIBUTING.md)
- [DEPLOY.md](DEPLOY.md)
- [docs/architecture.md](docs/architecture.md)
- [docs/api-overview.md](docs/api-overview.md)
- [docs/sqlserver-corporate-migration-guide.md](docs/sqlserver-corporate-migration-guide.md)
- [infra/README.md](infra/README.md)
