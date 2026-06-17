# Project Status

**Updated:** 2026-06-17 · **Branch:** `feat/scaffold-walking-skeleton`

Status of the build against the phased plan in [design/05-implementation-plan.md](./design/05-implementation-plan.md).
Legend: ✅ done & verified · 🟡 partial · ⬜ not started.

## Phase summary

| Phase | Scope | Status |
|------|-------|--------|
| 0 — Foundations / walking skeleton | Solution, BuildingBlocks, infra, CI/CD, observability | 🟡 ~80% |
| 1 — Identity, Org & Access | Org tree, users, roles, 2-D authz, Keycloak | ✅ ~90% |
| 2 — Catalog / Master Data | Indicators, indicator sets, templates, periods | 🟡 ~25% |
| 3 — Sector Data | Observations + rich entities (clusters, petrol, violations) | ⬜ **next (tomorrow)** |
| 4 — Reporting & Workflow | Campaigns, approval saga/state machine | ⬜ |
| 5 — Analytics & Dashboards | Read models, aggregate reports | ⬜ |
| 6 — Integration, Security L3, Go-live | LGSP/NDXP, hardening, data migration | ⬜ |

## What is built (detail)

### Phase 0 — Foundations 🟡
- ✅ DDD modular-monolith solution (.NET 10), `BuildingBlocks` (Domain/Application/Infrastructure/Web)
- ✅ CQRS pipeline: validation, logging, **authorization** behaviors; `Result`, `Specification`, `PredicateBuilder`, `PagedResult`, outbox entity
- ✅ API host (module composition, Swagger+bearer, ProblemDetails) + Worker host
- ✅ Docker Compose: Postgres/PostGIS, Redis, RabbitMQ, MinIO, Keycloak, api, worker, frontend(Nginx)
- ✅ EF Core migrations + dev auto-migrate; Serilog console logging
- ⬜ CI/CD pipeline (GitHub Actions) — not yet
- ⬜ OpenTelemetry traces + Prometheus/Grafana metrics + Seq/Loki logs — not yet
- ⬜ Real outbox dispatcher (Worker has a placeholder); Redis/MinIO not yet used in code

### Phase 1 — Identity, Org & Access ✅
- ✅ Org-unit tree (multi-level, create/list/search) — path stored as text
- ✅ Users (create/list/search) and Roles (create/list/search, permission sets)
- ✅ **Function-scope** authz (`IPermissionAuthorized` + pipeline behavior)
- ✅ **Data-scope** authz resolved from the user's assigned unit, **DB-driven** via a claims transformation
- ✅ Keycloak OIDC (realm import, SPA login/logout, API JWT validation, demo users)
- ✅ Dev seeder (org tree, ADMIN/SPECIALIST roles, `superadmin`/`chuyenvien`)
- ⬜ Reset-password-to-default (Keycloak admin API) — endpoint not built
- ⬜ Audit logging (audit behavior + AuditSystem context) — deferred to its own phase
- ⬜ `ltree` column type + GIST index (currently `text` + prefix match); update/delete endpoints

### Phase 2 — Catalog 🟡
- ✅ Versioned `Indicator` aggregate (Circular 33/2022) — create/list/search on `catalog` schema
- ⬜ Indicator sets (bộ chỉ tiêu), report templates, reporting periods, administrative-unit catalog, classification catalogs, batch import

### Frontend
- ✅ **Light theme is the default for all pages**; auth-gated; bearer-token interceptor
- ✅ Pages: Org Units, Users, Roles, Indicators (list / search / create)
- ⬜ Edit & delete UI, detail views, maps (GIS), dashboards/charts

## Verification (current)
- `dotnet build` → 0 warnings / 0 errors; no known-vulnerable dependencies
- `dotnet test` → **15/15 pass** (domain + authorization behavior)
- `npm run build` (frontend) → OK
- Runtime smoke test → `identity` + `catalog` schemas migrate, dev seed applies, all endpoints reject anonymous callers (401)

## Next up — Phase 3 Sector Data (tomorrow)
Planned scope (replicating the IdentityAccess/Catalog module template):
- ⬜ Generic, partitioned `IndicatorObservation` aggregate (numeric stats keyed by indicator + unit + period) — consumes Catalog indicators
- ⬜ Rich entities with PostGIS geometry: `IndustrialCluster`, `PetroleumStation`, `CommerceLocation`, `MarketViolationCase`
- ⬜ Excel/XML import (Strategy pattern), validation, detail views
- ⬜ `SectorData` module on its own `sector` schema + migration; register in host
- ⬜ Data-scope applied to observations/entities by org unit; frontend pages + map view

## Commits so far (this branch)
- `e1544f2` docs: design baseline
- `7d3df1b` feat: scaffold walking skeleton
- `cfc7d19` feat(auth): Keycloak OIDC + two-dimensional authorization
- `7a1ad92` feat: Users & Roles, DB-driven authorization, Catalog context

## Run it
```bash
cd deploy && cp .env.example .env && docker compose up -d --build
# frontend :8081 · API :8080/swagger · Keycloak :8090
# log in as superadmin/admin (sees all) or chuyenvien/chuyenvien (scoped to its unit)
```
