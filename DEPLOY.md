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
SQL Server + Active Directory
```

## Componentes

### Aplicacao web

- ASP.NET Core MVC em `mvc/src/Checklist.Mvc`
- renderizacao server-side com controllers e Razor Views
- rotas administrativas, operacionais e STP no mesmo processo

### Camada de aplicacao

- casos de uso em `mvc/src/Checklist.Application`
- DTOs e contratos de leitura/escrita

### Infraestrutura

- EF Core + SQL Server em `mvc/src/Checklist.Infrastructure`
- `AppDbContext` (herda de `IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>`)
- servicos de autenticacao e persistencia
- integracao com Active Directory obrigatoria para supervisor, inspetor e operador

### Banco de dados

- SQL Server para persistencia principal
- banco em memoria apenas como fallback local sem conexao configurada (nao suporta migrations)

## Autenticacao

Todo login de Supervisor, Inspetor e Operador e validado direto no Active Directory via
`ActiveDirectoryService.AuthenticateAD` (`System.DirectoryServices.AccountManagement`).

### Usuario Master

Depois que o schema estiver migrado, criar o master rodando este script no banco de producao
(trocando `SEU_LOGIN_AD` pelo login do administrador):

```sql
DECLARE @Login nvarchar(256) = N'LoginAdministrativo';
DECLARE @UserId uniqueidentifier = NEWID();
DECLARE @RoleId uniqueidentifier = NEWID();

INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
VALUES (@RoleId, N'Master', N'MASTER', CONVERT(nvarchar(max), NEWID()));

INSERT INTO AspNetUsers (
    Id, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
    LockoutEnd, LockoutEnabled, AccessFailedCount
)
VALUES (
    @UserId, @Login, UPPER(@Login), NULL, NULL,
    0, NULL, CONVERT(nvarchar(max), NEWID()), CONVERT(nvarchar(max), NEWID()),
    NULL, 0, 0,
    NULL, 0, 0
);

INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@UserId, @RoleId);
```

Para adicionar outro master depois, repetir só o bloco de `AspNetUsers`/`AspNetUserRoles`
reaproveitando o mesmo `@RoleId` (nao recriar a role).

Depois de logado como master, o cadastro de Setores, Supervisores, Inspetores e Operadores e feito
inteiramente pelas telas do Master


## Validacao

### Master

- acesso a `/account/login` com login e senha do AD
- redirecionamento para Master → Setores
- cadastro de setor, supervisor, inspetor e operador funcionando

### Supervisor

- acesso a `/account/login`
- autenticacao bem sucedida via AD
- dashboard carrega
- catalogos carregam
- itens non-compliant carregam

### Operador

- acesso a `/operacao`
- redirecionamento correto para `/operador/login`
- checklist abre por QR ID (digitado ou lido pela camera)
- checklist envia com assinatura

### STP

- dashboard STP
- cadastro de areas
- checklist STP
- documentos
