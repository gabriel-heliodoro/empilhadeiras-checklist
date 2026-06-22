# Setup and Deploy

Documento de referencia para configuracao e execucao do CheckFlow na arquitetura MVC atual com SQL Server.

## Topologia atual

```text
Usuario
  |
  v
Checklist.Mvc
  |
  v
Checklist.Application
  |
  v
Checklist.Infrastructure
  |
  v
SQL Server
```

## Componentes

### Aplicacao web

- ASP.NET Core MVC em `mvc/src/Checklist.Mvc`
- renderizacao server-side com controllers e Razor Views
- autenticacao por cookie
- rotas administrativas, operacionais e STP no mesmo processo

### Camada de aplicacao

- casos de uso em `mvc/src/Checklist.Application`
- DTOs e contratos de leitura/escrita

### Infraestrutura

- EF Core + SQL Server em `mvc/src/Checklist.Infrastructure`
- `AppDbContext`
- servicos de autenticacao e persistencia
- integracao com Active Directory para supervisor quando o host e Windows

### Banco de dados

- SQL Server para persistencia principal
- banco em memoria apenas como fallback local sem conexao configurada

## Modos de deploy

### 1. Execucao local ou na rede interna com SQL Server existente

Esse e o modo recomendado para o seu cenario atual.

Requisitos:

- host Windows
- acesso ao SQL Server local ou corporativo
- `ConnectionStrings__Default` configurada
- `Authentication__Mode=ActiveDirectory` quando quiser validar supervisor no AD real

Exemplo:

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
Authentication__Mode=ActiveDirectory
ActiveDirectory__Domain=""""
ActiveDirectory__Container=""
```

## Variaveis principais

### Banco

```env
ConnectionStrings__Default=Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

### Autenticacao

```env
Authentication__Mode=ActiveDirectory
ActiveDirectory__Domain=""
ActiveDirectory__Container=""
```

### Bind HTTP

```env
ASPNETCORE_URLS=http://0.0.0.0:8080
```

## Sequencia de deploy

1. Configurar variaveis de ambiente da aplicacao MVC
2. Validar conectividade com o SQL Server
3. Definir o modo de autenticacao
4. Subir a aplicacao
5. Validar login administrativo
6. Validar login operacional
7. Validar fluxos criticos

## Validacao recomendada

### Supervisor

- acesso a `/account/login`
- autenticacao bem sucedida
- dashboard carrega
- catalogos carregam
- itens non-compliant carregam

### Operador

- acesso a `/operacao`
- redirecionamento correto para `/operador/login`
- checklist abre por QR ID
- checklist envia com assinatura

### STP

- dashboard STP
- cadastro de areas
- checklist STP
- documentos

## Observacoes operacionais

- Nao existe endpoint `/health` dedicado na linha MVC atual.
- O bootstrap local usa `EnsureCreatedAsync` quando o schema ainda nao existe.
- O projeto nao depende mais de Docker Compose para o fluxo local.

## Referencias

- [README.md](README.md)
- [infra/README.md](infra/README.md)
- [docs/architecture.md](docs/architecture.md)
- [docs/api-overview.md](docs/api-overview.md)
- [docs/sqlserver-corporate-migration-guide.md](docs/sqlserver-corporate-migration-guide.md)
