# Checklist Status for `meu-omni`

This note tracks the implementation status after reviewing `documents/Checklist_thiet_ke_codebase_MasterCare.docx`.

## Implemented in this pass

- `ARC-01` to `ARC-04`: the solution remains split into domain, application, infrastructure, and API layers with business rules outside controllers.
- `TEN-03` to `TEN-05`, `MID-02`, `QRY-01`, `QRY-02`, `QRY-03`: tenant context is resolved via `X-Tenant-Id` or token claim, and tenant-scoped entities are isolated automatically through an EF Core global query filter.
- `AUT-01` to `AUT-07`, `MID-03`: JWT auth, permission guard, role guard, and deny-by-default protection are wired in the API layer.
- `TRX-01` to `TRX-03`, `MRG-04`: write use cases now run through an explicit application-layer transaction manager.
- `DB-01` to `DB-03`, `MRG-05`: EF Core DbContext and transaction manager centrally manage database access.
- `AUD-01` to `AUD-04`, `MRG-06`: audit logs now include tenant id, module, actor id, correlation id, before/after, and reason for sensitive write flows.
- `API-01`, `API-03`, `MRG-07`: Swagger is enabled with auth and permission metadata for protected endpoints.
- `CFG-01` to `CFG-04`, `MRG-08`: runtime config now expects env/config-provider supplied connection string and JWT settings; placeholders and `.env.example` were added.
- `DDD-01` to `DDD-05`: aggregates, invariants, and repository boundaries remain aligned with the DDD structure already in place.
- `MID-01`, `MID-04`, `MID-05`: correlation id and exception handling middleware are in place without business logic leakage.
- `MRG-09`: a baseline automated self-test project covers core domain invariants.

## Implemented with caveats

- `TEN-01`, `TEN-07`, `MRG-01`: tenant ids and tenant-aware unique indexes were added in the EF model. Existing databases created before this change still need schema alignment if they are not recreated.
- Tenant-scoped startup/seeding and admin maintenance flows must explicitly disable tenant filtering when they need cross-tenant access; `DatabaseSeeder` already does this, but broader operational scripts still need the same care.
- `AUD-05`: audit persistence exists, but a dedicated audit read API/UI with permissioned querying is not implemented yet.
- `API-02`: Swagger schemas are present through DTOs and XML generation is enabled, but not every endpoint has rich XML summary/example documentation yet.
- `QLT-02`: domain/application validation is strong, but request DTO validation can still be expanded with more explicit data annotations.
- `QLT-04`: baseline automated rule tests exist via `MeuOmni.SelfTests`, but they are not a full unit-test framework.

## Still remaining for full checklist parity

- `TEN-02`: a formal document/table classifying every table as global/shared vs tenant-scoped is still missing.
- `TEN-06`: tenant-focused index strategy can be reviewed further once real production query patterns are known.
- `QRY-05`, `QLT-05`: cross-tenant isolation and rollback/audit integration tests are not implemented yet.
- `TRX-04` to `TRX-06`: no outbox/event pattern, idempotency layer, or concurrency token strategy has been added yet.
- `DB-04`: database timeout policy and structured DB error logging can still be strengthened.
- `API-04`, `API-05`: module grouping and request/response examples can be expanded further in Swagger.
- `CFG-06`: the committed source now uses placeholders, but secret hygiene in external deployment environments still depends on ops setup.
- `DDD-06`: no domain-event based side effect coordination has been added yet.
- `QLT-01`: error responses are standardized structurally, but a richer error-code catalog is still missing.
- `QLT-03`: request id is logged/scoped, but broader structured logging enrichment with tenant and user dimensions can be expanded.

## Operational note

- Login and protected requests now require tenant context. For local demo data, use `X-Tenant-Id: mastercare-demo`.

