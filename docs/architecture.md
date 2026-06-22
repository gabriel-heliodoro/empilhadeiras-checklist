# Architecture

Visao estrutural do CheckFlow na arquitetura atual.

## Visao geral

A solucao esta organizada em quatro camadas dentro da linha MVC:

1. `Checklist.Mvc`
2. `Checklist.Application`
3. `Checklist.Domain`
4. `Checklist.Infrastructure`

Persistencia principal:

5. SQL Server

## Camadas

### 1. Presentation - `Checklist.Mvc`

Responsabilidades:

- controllers HTTP
- views Razor
- fluxo de navegacao
- formularios
- cookies de autenticacao

Exemplos de modulos:

- `AccountController`
- `OperatorAccountController`
- `OperationController`
- `OperatorController`
- `CatalogController`
- `HomeController`
- `NonOkController`
- `StpController`

### 2. Application - `Checklist.Application`

Responsabilidades:

- DTOs
- contratos de leitura e escrita
- abstracoes de autenticacao
- servicos orientados a caso de uso

Essa camada nao conhece Razor nem detalhes de infraestrutura concreta.

### 3. Domain - `Checklist.Domain`

Responsabilidades:

- conceitos centrais do negocio
- tipos compartilhados
- invariantes do dominio que nao dependem de UI nem de persistencia

### 4. Infrastructure - `Checklist.Infrastructure`

Responsabilidades:

- `AppDbContext`
- modelos persistidos
- consultas e comandos em EF Core
- integracao com SQL Server
- autenticacao administrativa e operacional
- integracao com Active Directory

## Autenticacao

### Supervisor

- cookie de autenticacao
- login em `/account/login`
- modo `ActiveDirectory` para AD real
- modo `DevelopmentStub` para ambiente de desenvolvimento

Importante:

- a validacao AD real so e registrada em Windows
- em Linux, a validacao AD real continua indisponivel

### Operador

- cookie de autenticacao proprio
- login em `/operador/login`
- senha hash persistida no banco
- fluxo de primeiro acesso suportado

## Persistencia

### Banco principal

- SQL Server
- EF Core
- `AppDbContext` em `mvc/src/Checklist.Infrastructure/Data/AppDbContext.cs`

### Fallback local

Quando nenhuma conexao e configurada:

- o sistema usa banco em memoria
- o bootstrap local cria dados minimos para navegacao

Esse modo nao substitui validacao real com SQL Server.

## Fluxos principais

### Operacao

1. operador acessa `/operacao`
2. se necessario, faz login em `/operador/login`
3. informa QR ID ou busca equipamento
4. preenche checklist
5. assina e envia

### Supervisao

1. supervisor acessa `/account/login`
2. navega para dashboard
3. consulta historico e detalhe de checklists
4. trata itens non-compliant
5. administra catalogos e fechamentos

### STP

1. inspetor autenticado acessa dashboard STP
2. opera areas, checklists e documentos
3. mantem historico e rastreabilidade

## Estado da migracao

- `mvc/` e a linha ativa
- a documentacao operacional deve apontar para `mvc/src/Checklist.Mvc`
