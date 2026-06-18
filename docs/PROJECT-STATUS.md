# Project Status

**Updated:** 2026-06-18 · **Branch:** `feat/scaffold-walking-skeleton`

Status of the build against the phased plan in [design/05-implementation-plan.md](./design/05-implementation-plan.md).
Legend: ✅ done & verified · 🟡 partial · ⬜ not started.

## Phase summary

| Phase | Scope | Status |
|------|-------|--------|
| 0 — Foundations / walking skeleton | Solution, BuildingBlocks, infra, CI/CD, observability | 🟡 ~95% |
| 1 — Identity, Org & Access | Org tree, users, roles, 2-D authz, Keycloak, audit log | ✅ ~95% |
| 2 — Catalog / Master Data | Indicators, indicator sets, templates, periods | 🟡 ~25% |
| 3 — Sector Data | Observations + rich entities (clusters, violations, petrol, commerce, e-comm) | 🟡 ~85% |
| 4 — Reporting & Workflow | Campaigns, approval saga/state machine, notifications | 🟡 ~95% |
| 5 — Analytics & Dashboards | Read models, aggregate reports | 🟡 ~75% |
| 6 — Integration, Security L3, Go-live | LGSP/NDXP, hardening, data migration | 🟡 ~55% |

## What is built (detail)

### Phase 0 — Foundations 🟡
- ✅ DDD modular-monolith solution (.NET 10), `BuildingBlocks` (Domain/Application/Infrastructure/Web)
- ✅ CQRS pipeline: validation, logging, **authorization** behaviors; `Result`, `Specification`, `PredicateBuilder`, `PagedResult`, outbox entity
- ✅ API host (module composition, Swagger+bearer, ProblemDetails) + Worker host
- ✅ Docker Compose: Postgres/PostGIS, Redis, RabbitMQ, MinIO, Keycloak, api, worker, frontend(Nginx)
- ✅ EF Core migrations + dev auto-migrate; Serilog console logging
- ✅ **Transactional outbox**: SaveChanges interceptor writes domain events to per-context outbox
  tables; an `OutboxProcessor<TContext>` background service drains them and publishes in-process via
  MediatR (verified: seeded `OrgUnitCreated` events written + processed)
- ✅ **CI/CD**: GitHub Actions (`.github/workflows/ci.yml`) — builds + tests the backend on .NET 10
  and builds the frontend on every push/PR to main
- ✅ **Integration tests** via Testcontainers (real PostgreSQL): outbox interceptor + data-scope spec
- ✅ **Observability**: OpenTelemetry traces (ASP.NET Core + Npgsql, OTLP export when configured) +
  metrics at `/metrics` (Prometheus); health split (`/health/live`, `/health/ready` with a DB check)
- ⬜ Seq/Loki log aggregation + Grafana dashboards (needs a collector); Redis/MinIO not yet used in
  code; RabbitMQ delivery (Worker) is the future cross-service path

### Phase 1 — Identity, Org & Access ✅
- ✅ Org-unit tree (multi-level, create/list/search) — path stored as text
- ✅ Users (create/list/search) and Roles (create/list/search, permission sets)
- ✅ **Function-scope** authz (`IPermissionAuthorized` + pipeline behavior)
- ✅ **Data-scope** authz resolved from the user's assigned unit, **DB-driven** via a claims transformation
- ✅ Keycloak OIDC (realm import, SPA login/logout, API JWT validation, demo users)
- ✅ Dev seeder (org tree, ADMIN/SPECIALIST roles, `superadmin`/`chuyenvien`)
- ✅ **Audit logging** (design G1): an `AuditBehavior` records every command (actor, action, JSON
  payload, outcome) to the **AuditSystem** context (`audit` schema, jsonb payload); searchable API + UI
- ⬜ Reset-password-to-default (Keycloak admin API) — endpoint not built
- ⬜ `ltree` column type + GIST index (currently `text` + prefix match); update/delete endpoints

### Phase 2 — Catalog 🟡
- ✅ Versioned `Indicator` aggregate (Circular 33/2022) — create/list/search on `catalog` schema
- ⬜ Indicator sets (bộ chỉ tiêu), report templates, reporting periods, administrative-unit catalog, classification catalogs, batch import

### Phase 3 — Sector Data 🟡
- ✅ Generic `IndicatorObservation` aggregate (numeric stats by indicator + unit + period), data-scoped by unit
- ✅ `IndustrialCluster` rich entity with **PostGIS** Point geometry (SRID 4326) + GIST spatial index
- ✅ Data-scope by **org-unit id** (user's unit + descendants) — provider/claims now emit `scope_unit`
- ✅ **Market-violation cases** (hồ sơ vi phạm) — both groups (prohibited/counterfeit, food-safety),
  case lifecycle (reported → handling → resolved), data-scoped by unit
- ✅ **Petroleum stations** & **commerce locations** (markets/supermarkets/malls/convenience stores)
  — rich entities with PostGIS Point geometry; **e-commerce participants** (platforms + goods)
- ✅ `SectorData` module on its own `sector` schema (6 entity tables, 3 PostGIS geometry columns);
  create/list endpoints; registered in host
- ⬜ Excel/XML batch import; observation submit/approve workflow hooks; map view in UI

### Phase 4 — Reporting & Workflow 🟡
- ✅ `ReportingCampaign` (kỳ báo cáo) — create/list
- ✅ `ReportSubmission` **state machine**: Draft → Submitted → UnderReview → PendingApproval →
  Approved, with Return/Reject/Reopen; every transition guarded + recorded in owned history
- ✅ One action endpoint for all transitions; per-action permission (commune `submit`, specialist
  `review`, leader `approve`) enforced by the pipeline; data-scoped by unit
- ✅ `Reporting` module on its own `reporting` schema (campaign, report_submission, report_transition);
  domain events raised for each transition (`ReportStateChanged`)
- ✅ **Notification saga**: `ReportStateChanged` flows through the outbox to a **Notifications**
  context (own `notifications` schema) that records a notification; exposed via API + a header bell
  (unread badge) and notifications page
- ⬜ Per-user notification routing (currently a shared activity feed); RabbitMQ cross-service delivery
- ⬜ Bind report content to Catalog templates / SectorData observations (auto-extract)

### Phase 5 — Analytics & Dashboards 🟡
- ✅ `Analytics` context (read-only, no schema): CQRS read side via Dapper aggregate queries over the
  operational schemas, **data-scoped** by org unit (super-admin sees all)
- ✅ Endpoints: leadership **dashboard** (cross-domain counts), violations summary (by group/status +
  total fines), reporting summary (submissions by state)
- ⬜ Materialized views refreshed on events (currently live queries); charts; more aggregate reports
  (industry/commerce per Circular-34 templates); export

### Phase 6 — Integration, Security, Go-live 🟡
- ✅ **Integration** context (`integration` schema): data-sharing **service registry** with
  Registered→Published→Revoked lifecycle (Decree 47/2020); **connection-status** API (level-1 DB probe
  + level-2 published services) with history (retained ≥ 3 months)
- ✅ **AuditSystem** + audit behavior (see Phase 1) — a Level-3 control
- ⬜ Real LGSP/NDXP connectors + XML/JSON data-exchange feeds (registry + ACL scaffolding in place)
- ⬜ Security Level-3 hardening checklist + assessment; **legacy data migration** (Doc 04 §7); go-live

### Frontend
- ✅ **Light theme is the default for all pages**; auth-gated; bearer-token interceptor
- ✅ Pages: Org Units, Users, Roles, Indicators, Industrial Clusters, Observations, Market Violations,
  Petroleum Stations, Commerce Locations, E-commerce Participants (list / search / create)
- ✅ **Campaigns** + **Submissions** (workflow action buttons per state + transition-history timeline)
- ✅ **Notifications** page + header bell with unread badge
- ✅ **Dashboard** (landing page): statistic cards + reporting/violation breakdown tables
- ✅ **Audit log** page (search by user/action, expandable payload)
- ✅ **Integration** page (connection-status panel + data-sharing service registry with publish/revoke)
- ⬜ Edit & delete UI, detail views, interactive map (GIS), dashboards/charts

## Verification (current)
- `dotnet build` → 0 warnings / 0 errors; no known-vulnerable dependencies
- `dotnet test` → **40/40 pass** — 38 unit + **2 integration** (Testcontainers PostgreSQL: outbox
  interceptor, data-scope specification)
- Outbox pipeline verified at runtime: seeded `OrgUnitCreated` events written to the outbox and
  drained by the processor (`total=2, processed=2`)
- `npm run build` (frontend) → OK
- Runtime smoke test → `identity` + `catalog` + `sector` schemas migrate, dev seed applies, PostGIS
  geometry column + GIST index created, all endpoints reject anonymous callers (401)

## Next up
All 7 designed bounded contexts are built; CI/CD, integration tests, and observability are in place.
Remaining work is hardening, real integrations, and polish:
- Security **Level-3 hardening** checklist + assessment readiness; **legacy data migration** (Doc 04 §7)
- Real **LGSP/NDXP** connectors + XML/JSON data-exchange feeds; file/resource module (MinIO, UC-4)
- Log aggregation (Seq/Loki) + Grafana dashboards; per-user notification routing
- UX polish: dashboard charts, interactive **GIS map**, Excel/XML batch import, edit/delete + detail views
- Catalog completion (indicator sets, report templates, reporting periods)

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
