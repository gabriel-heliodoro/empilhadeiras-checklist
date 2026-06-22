# CheckFlow v1.0.0

## Documento Tecnico de Apresentacao

## 1. Objetivo

Este documento descreve a forma tecnica atual do CheckFlow depois da migracao para a linha MVC.

O foco aqui e:

- modelo atual de execucao
- estrutura ativa do projeto
- modos de autenticacao
- comportamento de persistencia
- restricoes operacionais

## 2. Arquitetura Atual

A solucao ativa esta concentrada em:

- `mvc/src/Checklist.Mvc`
- `mvc/src/Checklist.Application`
- `mvc/src/Checklist.Domain`
- `mvc/src/Checklist.Infrastructure`

Persistencia principal:

- SQL Server via EF Core

As antigas linhas de API e React nao sao mais o ponto de entrada em runtime do produto.

## 3. Modelo de Execucao

### 3.1 Ponto de entrada

Ponto de entrada atual:

- [Program.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Mvc/Program.cs)

A aplicacao MVC:

- registra controllers com views
- conecta os servicos de infraestrutura
- inicializa o bootstrap local
- usa autenticacao e autorizacao
- mapeia a rota MVC padrao

### 3.2 Persistencia

Contexto de banco atual:

- [AppDbContext.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Infrastructure/Data/AppDbContext.cs)

Comportamento:

- quando existe connection string de SQL Server, a aplicacao usa SQL Server
- quando nao existe conexao configurada, a aplicacao cai para banco em memoria para desenvolvimento local

Componente de bootstrap:

- [LocalMvcDatabaseBootstrapper.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Infrastructure/Services/LocalMvcDatabaseBootstrapper.cs)

Limitacao atual:

- a linha MVC ainda nao carrega um conjunto de migrations versionadas para um fluxo formal de producao

### 3.3 Autenticacao

Autenticacao de supervisor:

- baseada em cookie
- tela de login em `/account/login`
- modo `DevelopmentStub` para desenvolvimento local
- modo `ActiveDirectory` para validacao real no AD

Restricao importante:

- a validacao real de AD esta implementada somente para hosts Windows

Autenticacao de operador:

- baseada em cookie
- tela de login em `/operador/login`
- credenciais persistidas no SQL Server
- fluxo de primeiro acesso suportado

## 4. Familias de rotas ativas

Acesso administrativo:

- `/account/login`
- `/account/logout`

Acesso operacional:

- `/operacao`
- `/operador/login`
- `/operador`
- `/operador/checklists/...`

Supervisao:

- `/`
- `/checklists`
- `/non-ok`
- `/catalog/...`
- `/monthly-closures`

STP:

- `/stp/dashboard`
- `/stp/areas`
- `/stp/checklists`
- `/stp/documents`

## 5. Observacoes operacionais

- Nao existe endpoint `/health` dedicado na linha MVC neste momento.
- Docker Compose nao faz mais parte do fluxo principal local.
- Containers Linux servem para navegacao e demonstracao local, nao para validacao real de AD.

## 6. Direcao recomendada

O repositorio deve continuar convergindo para a linha MVC:

1. MVC como unico runtime documentado
2. assets de deploy apontando para `mvc/src/Checklist.Mvc`
3. o repositorio deve permanecer centrado apenas na linha MVC

Este documento deve ser lido como briefing do estado atual do projeto.
