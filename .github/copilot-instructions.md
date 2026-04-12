# Copilot Instructions for MeuOmni

## 1. Project context

This workspace is for `MeuOmni`, an omni-channel retail management system for MeU Solutions.

Current architecture direction:

- `DDD`
- `modular monolith`
- `database per module`

Business goals currently centered on:

- POS / in-store sales
- multi-channel order intake
- product, customer, and inventory management
- order lifecycle and payment flows
- future expansion to simple e-commerce

## 2. Source of truth

Use the actual repository structure as the source of truth.

Current active solution and source layout:

- main solution: `C:\Work\MeU Solutions\Project\meu-omni-solutions\MeuOmni.Modular.sln`
- main source: `C:\Work\MeU Solutions\Project\meu-omni-solutions\src-modular`

Important:

- `src/` is no longer present in the current workspace.
- Work should target the modular solution unless the user explicitly asks otherwise.
- If a document conflicts with the real repo structure, trust the repo structure first.

## 3. Current repo structure

Main folders:

- `src-modular/MeuOmni.Bootstrap`
- `src-modular/MeuOmni.BuildingBlocks`
- `src-modular/Modules/SalesChannel`
- `src-modular/Modules/SimpleCommerce`
- `tests/MeuOmni.SelfTests`
- `docs`
- `documents`

Main solution projects:

- `MeuOmni.Bootstrap`
- `MeuOmni.BuildingBlocks`
- `MeuOmni.Modules.SalesChannel.Domain`
- `MeuOmni.Modules.SalesChannel.Application`
- `MeuOmni.Modules.SalesChannel.Infrastructure`
- `MeuOmni.Modules.SalesChannel.Api`
- `MeuOmni.Modules.SimpleCommerce.Domain`
- `MeuOmni.Modules.SimpleCommerce.Application`
- `MeuOmni.Modules.SimpleCommerce.Infrastructure`
- `MeuOmni.Modules.SimpleCommerce.Api`

## 4. Module intent

### SalesChannel

This is the central module.

Responsibilities:

- receive orders from multiple channels
- normalize order handling into one core sales flow
- own sales order lifecycle
- support POS-oriented workflows over time

### SimpleCommerce

This is a supporting online channel, not a second order core.

Responsibilities:

- storefront
- public catalog experience
- simple online buying flow

Rule:

- successful checkout from `SimpleCommerce` should feed into `SalesChannel`
- do not create a separate competing order core in `SimpleCommerce`

## 5. Layer responsibilities

### Bootstrap

`MeuOmni.Bootstrap` is the composition root only.

Allowed:

- application startup
- DI wiring
- swagger setup
- module registration
- host-level middleware
- database initialization per module

Not allowed:

- business rules
- domain decisions
- module-specific business branching that belongs elsewhere

### BuildingBlocks

`MeuOmni.BuildingBlocks` is the shared kernel.

Keep only minimal shared abstractions here, such as:

- domain base types
- module contracts
- cross-cutting primitives with clear reuse value

Do not move module business logic into shared code just to avoid duplication.

### Domain

Domain is where business rules must live.

Contains:

- entities
- aggregates
- value rules
- invariants
- repository interfaces

### Application

Application orchestrates use cases.

Contains:

- commands
- DTOs
- application services
- coordination across domain objects

Application should not contain persistence details.

### Infrastructure

Infrastructure contains technical implementation details.

Contains:

- DbContext
- repository implementations
- persistence wiring
- module registration

### Api

Api should stay thin.

Contains:

- controllers
- request/response transport mapping
- delegation to application services

## 6. Architecture rules

These rules are mandatory unless the user explicitly asks to change architecture:

1. Keep business logic in `Domain`.
2. Do not put business rules in controllers.
3. Do not join module databases directly.
4. Do not create foreign keys across modules or databases.
5. Prefer contracts, events, or read models for cross-module coordination.
6. Keep module ownership clear. Each module owns its own data and lifecycle.
7. Avoid leaking one module's infrastructure into another module.

## 6.1 Security and tenant defaults for all modules

These rules are mandatory for every new module and controller scaffold:

1. All protected API routes must carry tenant context through `X-Tenant-Id`.
2. All module endpoints under `/api/modules/*` must declare at least one `[RequireRole]` or `[RequirePermission]`.
3. Do not hard-code permission strings in controllers. Always use `MeuOmni.BuildingBlocks.Security.PermissionCodes`.
4. API controllers should inherit `MeuOmni.BuildingBlocks.Web.BaseApiController`.
5. Application services must resolve and verify tenant via `TenantContextGuard.ResolveTenantId(...)`.
6. Tenant-scoped EF Core contexts must inherit `MeuOmni.BuildingBlocks.Persistence.TenantAwareDbContext`.
7. Tenant-scoped EF Core models must mark `TenantId` as required and call `ApplyTenantQueryFilters(modelBuilder)`.
8. Roles and permissions must come from authenticated token claims, not custom headers.
9. New modules should plug into the existing host-level middleware chain instead of duplicating tenant, role, or permission checks locally.

## 7. Runtime and configuration facts

Current host behavior in `src-modular/MeuOmni.Bootstrap/Program.cs`:

- registers module controllers from `SalesChannel` and `SimpleCommerce`
- exposes swagger
- exposes `GET /`
- returns service metadata:
  - `service = "meu-omni-modular"`
  - `architecture = "modular-monolith"`
  - `databaseStrategy = "database-per-module"`

Current module database configuration is expected under:

- `Modules:SalesChannel:Database:ConnectionString`
- `Modules:SimpleCommerce:Database:ConnectionString`

Default local config is in:

- `src-modular/MeuOmni.Bootstrap/appsettings.json`

## 8. Documentation priority

When you need context, read in this order:

1. actual code in `src-modular/`
2. `MeuOmni.Modular.sln`
3. `docs/architecture.md`
4. `docs/modular-monolith-separate-databases.md`
5. `README.md`
6. business documents under `documents/`

Notes:

- Some markdown files in the repo may have stale assumptions or bad encoding.
- Prefer current code and solution structure over older migration notes.

## 9. Tests and reliability notes

`tests/MeuOmni.SelfTests` exists in the repo, but it is not part of the modular solution and may lag behind the current modular structure.

Treat it as supporting context, not the primary source of truth.

Before relying on tests or references there:

- verify project references still match the current repo
- prefer validating against `MeuOmni.Modular.sln`

## 10. How Copilot should help

When making suggestions or code changes:

- default to the modular architecture
- preserve module boundaries
- keep API thin and Domain rich
- prefer incremental, buildable changes
- update related documentation if architecture or workflow meaningfully changes
- when scaffolding new modules, include tenant, role, permission, and `BaseApiController` conventions from the start

When uncertain:

- inspect the current module code first
- avoid inventing missing layers or cross-module shortcuts
- ask for clarification only if the architecture impact is non-trivial

## 11. Preferred commands

Use these commands by default:

```bash
dotnet build MeuOmni.Modular.sln
dotnet run --project src-modular/MeuOmni.Bootstrap/MeuOmni.Bootstrap.csproj
```

If configuration is needed, check:

- `.env.example`
- `NuGet.Config`
- `src-modular/MeuOmni.Bootstrap/appsettings.json`

## 12. Desired outcome

Copilot should behave as if:

- `MeuOmni.Modular.sln` is the main system
- `src-modular/` is the authoritative implementation
- `SalesChannel` is the current core business module
- `SimpleCommerce` is an online channel feeding the core sales flow
- architecture integrity matters as much as feature delivery
