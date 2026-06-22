

## SQL Server local ou corporativo

Rode o MVC diretamente:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__Default="Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
dotnet run --project ..\mvc\src\Checklist.Mvc\Checklist.Mvc.csproj --urls http://localhost:5204
```

Se a connection string ja estiver em `appsettings.Development.json`, voce pode apenas executar:

```powershell
dotnet run --project ..\mvc\src\Checklist.Mvc\Checklist.Mvc.csproj --urls http://localhost:5204
```

## Observacoes importantes

- O login administrativo por Active Directory implementado hoje depende de Windows.
- Se nenhuma conexao for informada, o MVC pode cair para banco em memoria para desenvolvimento local.
- O bootstrap local cria schema e seed minimo via `EnsureCreatedAsync`.
