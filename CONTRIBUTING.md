# Contributing

## Objetivo

- codigo limpo
- mudancas pequenas e revisaveis
- build reproduzivel
- documentacao alinhada com a arquitetura MVC atual

## Regras basicas

1. Nao commitar artefatos gerados:
   - `bin/`
   - `obj/`
   - `dist/`
   - `artifacts/`
   - `temp-build-*`
3. mudancas pequenas por PR.
4. Mudancas de modelo devem atualizar `Checklist.Infrastructure` e a documentacao correspondente.
5. Mudancas operacionais devem atualizar `README.md`, `DEPLOY.md`, `infra/README.md` e os arquivos em `docs/` quando aplicavel.

## Branches

Sugestao:

- `feature/<nome-curto>`
- `fix/<nome-curto>`
- `refactor/<nome-curto>`
- `docs/<nome-curto>`

## Commits

Sugestao:

- `feat(mvc): add operator checklist flow`
- `fix(auth): correct supervisor cookie redirect`
- `refactor(catalog): split equipment form mapping`
- `docs(deploy): update mvc deployment guide`

## Checklist antes de abrir PR

1. Rodar build do MVC:

```powershell
dotnet build mvc/src/Checklist.Mvc/Checklist.Mvc.csproj
```

2. Se a mudanca tocar persistencia real, validar o fluxo com SQL configurado.

3. Se a mudanca tocar autenticacao administrativa, validar os dois cenarios:
   - `DevelopmentStub`
   - `ActiveDirectory` em Windows

4. Confirmar que nao ha artefatos gerados no `git status`.

5. Confirmar que `appsettings`, `.env` e scripts locais nao contem credenciais reais.

## Banco e schema

Estado atual da linha MVC:

- o projeto usa `AppDbContext` em `mvc/src/Checklist.Infrastructure`
- o bootstrap local usa `EnsureCreatedAsync`

Se migrations passarem a ser adotadas nesta linha, elas devem ficar em `mvc/src/Checklist.Infrastructure` e usar `mvc/src/Checklist.Mvc` como startup project.

## Comandos uteis

Rodar a aplicacao MVC:

```powershell
dotnet run --project mvc/src/Checklist.Mvc/Checklist.Mvc.csproj --urls http://localhost:5204
```

## Documentacao

Atualize estes arquivos quando aplicavel:

- `README.md`
- `DEPLOY.md`
- `infra/README.md`
- `docs/architecture.md`
- `docs/api-overview.md`
- `docs/mysql-corporate-migration-guide.md`
