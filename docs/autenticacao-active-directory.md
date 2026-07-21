# Autenticacao via Active Directory

Fluxo padrao de login no CheckFlow. Dois pontos de entrada (supervisor/admin/inspetor e operador)
que convergem no mesmo ponto de validacao contra o AD.

## Resumo

- Nao existe branch de codigo que desvie do AD por ambiente (dev/producao). A validacao e a mesma
  nos dois casos.
- Unica excecao: a conta master (`IsMaster = true`) autentica com senha local (hash), nunca via AD.
- Operador sempre autentica via AD. Ele precisa ja existir e estar `IsActive` no banco local antes -
  o AD so valida a senha, nao cria conta.

## 1. Entrada - Supervisor / Admin / Inspetor

`mvc/src/Checklist.mvc/Controllers/AccountController.cs`

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
{
    var result = await _authenticationService.AuthenticateAsync(model.Login, model.Password, cancellationToken);
    if (!result.Success || result.Value is null)
    {
        ModelState.AddModelError(string.Empty, result.Error ?? "Nao foi possivel autenticar o supervisor.");
        return View(model);
    }
```

## 2. Entrada - Operador

`mvc/src/Checklist.mvc/Controllers/OperatorAccountController.cs` - mesmo padrao, chamando
`IOperatorAuthenticationService.AuthenticateAsync`.

## 3. Validacao - Supervisor (so nao-master vai pro AD)

`mvc/src/Checklist.Infrastructure/Identity/SupervisorAuthenticationService.cs`

```csharp
var passwordValid = supervisor.IsMaster
    ? _passwordHashingService.VerifyPassword(normalizedPassword, supervisor.PasswordHash)
    : ActiveDirectoryService.AuthenticateAD(normalizedLogin, normalizedPassword);
```

## 4. Validacao - Operador (sempre AD)

`mvc/src/Checklist.Infrastructure/Identity/OperatorAuthenticationService.cs`

```csharp
var operador = await _dbContext.Operators
    .AsTracking()
    .Include(x => x.Sector)
    .FirstOrDefaultAsync(x => x.Login == normalizedLogin && x.IsActive, cancellationToken);

if (operador is null || !ActiveDirectoryService.AuthenticateAD(normalizedLogin, normalizedPassword))
{
    return Result<OperatorSessionDto>.Fail("Login ou senha invalidos.");
}
```

## 5. Ponto de convergencia - chamada real ao AD

`mvc/src/Checklist.Infrastructure/Services/ActiveDirectoryService.cs`

```csharp
public static bool AuthenticateAD(string user, string password)
{
    try
    {
        using (var context = folders is null
            ? new PrincipalContext(ContextType.Domain, dominio)
            : new PrincipalContext(ContextType.Domain, dominio, folders))
        {
            return context.ValidateCredentials(user, password);
        }
    }
    catch (Exception)
    {
        return false;
    }
}
```

Dominio e OU fixos no codigo:

- `dominio = "schott.org"`
- `folders = "OU=Users,OU=RI1,OU=BR,DC=schott,DC=org"`

## 6. Sessao - grava claims + cookie

- Supervisor: `AccountController.cs` grava o esquema `MvcAuthenticationSchemes.Supervisor`
  ("SupervisorCookie").
- Operador: `OperatorAccountController.cs` grava o esquema `MvcAuthenticationSchemes.Operator`
  ("OperatorCookie").

## 7. Registro dos dois esquemas de cookie + roteamento por path

`mvc/src/Checklist.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`

```csharp
.AddPolicyScheme(MvcAuthenticationSchemes.App, "App cookie scheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        return context.Request.Path.StartsWithSegments("/operador", StringComparison.OrdinalIgnoreCase)
            ? MvcAuthenticationSchemes.Operator
            : MvcAuthenticationSchemes.Supervisor;
    };
})
.AddCookie(MvcAuthenticationSchemes.Supervisor, options => { options.LoginPath = "/account/login"; /* ... */ })
.AddCookie(MvcAuthenticationSchemes.Operator, options => { options.LoginPath = "/operador/login"; /* ... */ });
```

Ou seja: qualquer request para `/operador/**` usa o cookie de operador; todo o resto usa o cookie de
supervisor.

## 8. Autorizacao - politicas leem os claims gravados

Mesmo arquivo do passo 7. Politicas de supervisor: `SectorSupervisorReady`, `SafetyWorkReady`,
`MaterialsInspectionReady`, `MasterReady`. Politicas de operador: `OperatorAuthenticated`,
`OperatorChecklistReady`. Cada uma faz `RequireAssertion` sobre claims como `IsMaster`, `UserType`,
`AccessModule`, `ForceChangePassword`.

## O que controla producao vs development

| Config | Producao (`appsettings.json`) | Dev (`appsettings.Development.json`) |
|---|---|---|
| `ConnectionStrings:Default` | vazio -> cai pra InMemory DB se nao setado por env var | SQL Express local |
| `Authentication:Mode` | `ActiveDirectory` | `DevelopmentStub` |
| `MasterAccount` | credenciais de producao | `master.teste` / `123456` |

`Authentication:Mode` **nao** troca a implementacao de autenticacao. Ele so controla se o
`LocalMvcDatabaseBootstrapper` semeia um supervisor de teste local
(`EnsureDevelopmentSupervisorAsync`, condicionado a `Mode == DevelopmentStub`). A validacao contra o
AD (`ActiveDirectoryService.AuthenticateAD`) roda igual em qualquer ambiente.
