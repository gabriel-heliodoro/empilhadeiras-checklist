# Route Overview

Este documento nao descreve mais uma Web API publica em JSON.
Na arquitetura atual, o CheckFlow expõe principalmente rotas MVC com Razor Views e formularios.

## Acesso administrativo

- `GET /account/login`
- `POST /account/login`
- `POST /account/logout`

Uso:

- autenticacao de supervisor, master e inspector
- cria sessao por cookie

## Acesso operacional

- `GET /operacao`
- `POST /operacao`

Uso:

- ponto de entrada simplificado para operador via QR ID
- redireciona para login do operador quando necessario

## Conta do operador

- `GET /operador/login`
- `POST /operador/login`
- `GET /operador/primeiro-acesso`
- `POST /operador/primeiro-acesso`
- `POST /operador/logout`

## Fluxo do operador

- `GET /operador`
- `GET /operador/checklists/{equipmentId}`
- `GET /operador/checklists/qr/{qrId}`
- `POST /operador/checklists/{equipmentId}`

## Dashboard e historico do supervisor

- `GET /`
- `GET /checklists`
- `GET /checklists/{id}`
- `GET /non-ok`
- `GET /non-ok/lista`
- `GET /non-ok/{id}`
- `POST /non-ok/{id}/atribuir`
- `POST /non-ok/{id}/tratativa`
- `POST /non-ok/{id}/concluir`

## Catalogos operacionais

- `GET /catalog/categories`
- `POST /catalog/categories`
- `POST /catalog/categories/{id}/delete`

- `GET /catalog/templates`
- `POST /catalog/templates`
- `POST /catalog/templates/{id}/delete`

- `GET /catalog/operators`
- `POST /catalog/operators`

- `GET /catalog/equipment`
- `POST /catalog/equipment`

## Fechamento mensal

- `GET /monthly-closures`
- `POST /monthly-closures/close`
- `GET /monthly-closures/{id}/download`

## Master

- `GET /master/sectors`
- `POST /master/sectors`
- `GET /master/supervisors`
- `POST /master/supervisors`
- `GET /master/inspectors`
- `POST /master/inspectors`

## STP

- `GET /stp/dashboard`
- `GET /stp/areas`
- `POST /stp/areas`
- `GET /stp/checklists/new`
- `POST /stp/checklists/new`
- `GET /stp/checklists`
- `GET /stp/checklists/{id}`

## Documentos STP

- `GET /stp/documents`
- `POST /stp/documents/companies`
- `GET /stp/documents/companies/{companyId}`
- `POST /stp/documents/companies/{companyId}`
- `POST /stp/documents/companies/{companyId}/upload`
- `GET /stp/documents/company-files/{documentId}`
- `POST /stp/documents/companies/{companyId}/employees`
- `GET /stp/documents/employees/{employeeId}`
- `POST /stp/documents/employees/{employeeId}`
- `POST /stp/documents/employees/{employeeId}/upload`
- `GET /stp/documents/employee-files/{documentId}`

## Observacoes

- Nao existe endpoint `/health` dedicado na linha MVC atual.
- As rotas acima retornam HTML, redirecionamentos, download de arquivo ou submissao de formulario, conforme o caso.
- Se no futuro uma API publica em JSON voltar a existir, ela deve ser documentada separadamente.
