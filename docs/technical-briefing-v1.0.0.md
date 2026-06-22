# CheckFlow v1.0.0

## Technical Briefing

## 1. Objective

This document describes the current technical shape of CheckFlow after the migration to the MVC application line.

It focuses on:

- current execution model
- active project structure
- authentication modes
- persistence behavior
- operational constraints

## 2. Current Architecture

The active solution is centered on:

- `mvc/src/Checklist.Mvc`
- `mvc/src/Checklist.Application`
- `mvc/src/Checklist.Domain`
- `mvc/src/Checklist.Infrastructure`

Primary persistence:

- SQL Server via EF Core

The previous API and React lines are no longer the runtime entry point of the product.

## 3. Runtime Model

### 3.1 Entry point

Current application entry point:

- [Program.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Mvc/Program.cs)

The MVC app:

- registers controllers with views
- wires infrastructure services
- initializes the local bootstrapper
- uses authentication and authorization middleware
- maps the default MVC route

### 3.2 Persistence

Current database context:

- [AppDbContext.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Infrastructure/Data/AppDbContext.cs)

Behavior:

- when a SQL Server connection string is available, the app uses SQL Server
- when no connection string is available, the app falls back to an in-memory database for local development

Bootstrap component:

- [LocalMvcDatabaseBootstrapper.cs](/c:/Users/Gabriel/Documents/empilhadeiras-checklist/mvc/src/Checklist.Infrastructure/Services/LocalMvcDatabaseBootstrapper.cs)

Current limitation:

- the MVC line does not yet carry a versioned migrations set for a formal production workflow

### 3.3 Authentication

Supervisor authentication:

- cookie-based
- login page in `/account/login`
- `DevelopmentStub` mode for local development
- `ActiveDirectory` mode for real AD validation

Important constraint:

- real AD validation is implemented only on Windows hosts

Operator authentication:

- cookie-based
- login page in `/operador/login`
- credentials stored in SQL Server
- first-access password change flow supported

## 4. Active Route Families

Administrative access:

- `/account/login`
- `/account/logout`

Operator access:

- `/operacao`
- `/operador/login`
- `/operador`
- `/operador/checklists/...`

Supervisor access:

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

## 5. Operational Notes

- There is no dedicated `/health` endpoint in the MVC line at the moment.
- Docker Compose is no longer part of the main local workflow.
- Linux containers are suitable for local navigation and demo flows, not for real AD validation.

## 6. Recommended Direction

The repository should keep converging around the MVC line:

1. MVC becomes the only documented runtime path
2. deployment assets target `mvc/src/Checklist.Mvc`
3. the repository should remain centered on the MVC line only

This document should be read as the current-state briefing of the project.
