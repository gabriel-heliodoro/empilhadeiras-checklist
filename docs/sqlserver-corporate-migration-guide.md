# SQL Server Configuration Guide

Documento de referencia para configuracao do CheckFlow MVC com SQL Server.

## Objetivo

Padronizar a aplicacao para:

- usar uma fonte clara de conexao
- evitar segredos versionados
- manter o MVC alinhado com o banco real

## Informacoes necessarias

Antes da configuracao, confirme:

1. host do banco
2. instancia ou porta
3. nome do banco
4. modo de autenticacao
5. usuario e senha, quando nao usar trusted connection
6. politica de criptografia

## Configuracao recomendada

Use uma connection string completa em:

```env
ConnectionStrings__Default=Server=;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

## Exemplo para o MVC

```json
{
  "ConnectionStrings": {
    "Default": "Server=DESKTOP-6AUG6QN\\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "Authentication": {
    "Mode": "ActiveDirectory"
  },
  "ActiveDirectory": {
    "Domain": "",
    "Container": ""
  }
}
```

## Passos

### 1. Montar a connection string

Monte a string com:

- servidor
- instancia ou porta
- database
- trusted connection ou usuario/senha
- encrypt
- trust server certificate, quando aplicavel

### 2. Configurar o MVC

Defina no ambiente:

```env
ConnectionStrings__Default=Server=DESKTOP-6AUG6QN\SQLEXPRESS;Database=CheckFlowDatabase;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
Authentication__Mode=ActiveDirectory
ActiveDirectory__Domain=
ActiveDirectory__Container=
```

### 3. Subir a aplicacao

```powershell
dotnet run --project mvc/src/Checklist.Mvc/Checklist.Mvc.csproj --urls http://localhost:5204
```

### 4. Validar o sistema

Checklist:

1. aplicacao sobe sem excecao
2. conexao com banco funciona
3. login administrativo funciona
4. login operacional funciona
5. dashboard funciona
6. checklist operacional funciona
7. STP funciona

## Estado atual de schema

Na linha MVC atual:

- o schema local e criado com `EnsureCreatedAsync`
- ainda nao existe pacote de migrations versionadas equivalente a um fluxo formal de producao

Isso significa que a configuracao de banco deve ser validada com cuidado antes de apontar para ambientes compartilhados.

## Politica de segredos

- nao versionar segredo
- nao versionar senha de banco
- nao versionar credencial real de AD
- nao usar credenciais reais em `appsettings.json` de producao

## Erros comuns

1. misturar trusted connection com usuario e senha na mesma string sem necessidade
2. esquecer `TrustServerCertificate=True` em ambiente local quando o certificado nao e confiavel
3. tentar validar AD real em Linux
4. subir a aplicacao sem validar acesso ao banco
5. assumir que banco em memoria cobre teste real de persistencia
