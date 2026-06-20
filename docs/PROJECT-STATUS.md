# Project Status

**Updated:** 2026-06-19 · **Branch:** `main`

Status of the build against the phased plan in [design/05-implementation-plan.md](./design/05-implementation-plan.md).
Legend: ✅ done & verified · 🟡 partial · ⬜ not started.

## Phase summary

| Phase | Scope | Status |
|------|-------|--------|
| 0 — Foundations / walking skeleton | Solution, BuildingBlocks, infra, CI/CD, observability | 🟡 ~95% |
| 1 — Identity, Org & Access | Org tree, users, roles, 2-D authz, Keycloak, audit log | ✅ ~95% |
| 2 — Catalog / Master Data | Indicators, indicator sets, templates, periods | ✅ ~90% |
| 3 — Sector Data | Observations + rich entities (clusters, violations, petrol, commerce, e-comm) | 🟡 ~85% |
| 4 — Reporting & Workflow | Campaigns, approval saga/state machine, notifications | 🟡 ~95% |
| 5 — Analytics & Dashboards | Read models, aggregate reports | 🟡 ~90% |
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
- ✅ **Integration tests** via Testcontainers (real PostgreSQL): outbox interceptor + ltree subtree/data-scope
- ✅ **Observability**: OpenTelemetry traces (ASP.NET Core + Npgsql, OTLP export when configured) +
  metrics at `/metrics` (Prometheus); health split (`/health/live`, `/health/ready` with a DB check)
- ✅ **MinIO** used in code (Files module, object storage); **Redis** used in code (distributed cache
  backing the per-request authorization-resolution cache; falls back to in-memory when not configured)
- ⬜ Seq/Loki log aggregation + Grafana dashboards (needs a collector); RabbitMQ delivery (Worker) is
  the future cross-service path

### Phase 1 — Identity, Org & Access ✅
- ✅ Org-unit tree (multi-level, create/list/search) — path stored as text
- ✅ Users (create/list/search) and Roles (create/list/search, permission sets)
- ✅ **Function-scope** authz (`IPermissionAuthorized` + pipeline behavior)
- ✅ **Data-scope** authz resolved from the user's assigned unit, **DB-driven** via a claims transformation
- ✅ Keycloak OIDC (realm import, SPA login/logout, API JWT validation, demo users)
- ✅ Dev seeder (org tree, ADMIN/SPECIALIST roles, `superadmin`/`chuyenvien`)
- ✅ **Audit logging** (design G1): an `AuditBehavior` records every command (actor, action, JSON
  payload, outcome) to the **AuditSystem** context (`audit` schema, jsonb payload); searchable API + UI
- ✅ **OrgUnit update / delete / detail** endpoints (delete blocked if the unit has children; all
  audited) — the reusable edit/delete/detail pattern, also applied to Catalog Indicator
- ✅ **Reset-password-to-default** via the Keycloak Admin REST API (`IIdentityProviderAdmin` →
  `KeycloakAdminClient`): admin endpoint sets a temporary default password; UI action on the Users
  page (returns the new password to hand over). Config under `Keycloak:*`
- ✅ Org-unit path stored as PostgreSQL **`ltree`** with a **GIST index**; subtree/data-scope resolved
  via the `<@` descendant operator (the list query then filters by the resolved unit ids) — verified
  end-to-end against real PostgreSQL

### Phase 2 — Catalog ✅
- ✅ Versioned `Indicator` aggregate (Circular 33/2022) — create/list/search on `catalog` schema
- ✅ **Indicator sets** (bộ chỉ tiêu, member indicators), **report templates** (biểu mẫu, owned
  ordered lines bound to indicators, Circular 34), **reporting-period definitions** (kỳ báo cáo)
- ✅ **Administrative-unit catalog** (đơn vị hành chính: province/district/commune, optional parent)
  and **classification catalogs** (danh mục phân loại — code-list schemes owning ordered items) on the
  `catalog` schema; full list/search/create/edit/delete/detail. New schemes = catalog rows, not migrations

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
- ✅ Interactive **GIS map** (Leaflet): clusters, petrol stations, commerce locations as toggleable
  color-coded layers over OpenStreetMap tiles
- ✅ **Excel/XML/CSV batch import** for all 6 sector entities (observations + 5 rich entities):
  server-side **Strategy** parsers (ClosedXML / `System.Xml` / CSV) behind a parser factory →
  generic `/api/sector/import/parse` preview; typed per-entity **bulk-create** endpoints with
  row-level validation, in-batch dedupe, and partial-success reporting. Client resolves codes→ids
  (no cross-context coupling) via a reusable preview-then-commit `ImportModal` + CSV template download
- ✅ **Observation approval workflow**: guarded `Draft → Submitted → Approved` (+ Return to draft)
  on `IndicatorObservation`; one action endpoint with per-action permission (`observations.submit`
  for commune, `observations.approve` for specialist/leader), data-scoped by unit + audited; UI action
  buttons per status on the Observations page

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
- ✅ **Audience-routed notifications** (no longer a shared feed): each `ReportStateChanged` is
  addressed by **target permission + org unit** (submit→reviewers, forward→approvers, others→submitter);
  the bell/list/mark-read filter to the caller's audience (broadcasts + permissions they hold within
  their data-scope) using their own claims — no cross-context user lookup
- ⬜ RabbitMQ cross-service delivery (Worker)
- ✅ **Auto-extract report content**: a submission's content lines are assembled from a Catalog
  **report template** + the unit's **observations** for the campaign's period (owned `ReportLine`
  collection bound to the submission, editable only while Draft). Cross-context read via a read-only
  Dapper **ACL** (`IReportContentSource`) — no compile-time coupling to Catalog/SectorData (mirrors
  Analytics). `POST /submissions/{id}/extract`; content shown in the submission detail; UI picker on Draft

### Phase 5 — Analytics & Dashboards 🟡
- ✅ `Analytics` context (read-only, no schema): CQRS read side via Dapper aggregate queries over the
  operational schemas, **data-scoped** by org unit (super-admin sees all)
- ✅ Endpoints: leadership **dashboard** (cross-domain counts), violations summary (by group/status +
  total fines), reporting summary (submissions by state)
- ✅ **More aggregate reports**: observations rolled up **by sector** (Circular-34 statistical aggregate,
  joins catalog.indicator) and commerce locations **by type** — data-scoped Dapper queries + charts
- ✅ **CSV export** of dashboard reports (client-side, UTF-8 BOM for Excel) via per-card download buttons
- ⬜ Materialized views refreshed on events (currently live queries — an optimization; data is low-millions)

### Phase 6 — Integration, Security, Go-live 🟡
- ✅ **Integration** context (`integration` schema): data-sharing **service registry** with
  Registered→Published→Revoked lifecycle (Decree 47/2020); **connection-status** API (level-1 DB probe
  + level-2 published services) with history (retained ≥ 3 months)
- ✅ **AuditSystem** + audit behavior (see Phase 1) — a Level-3 control
- ✅ **Files & resources module** (UC-4): a `Files` context (`files` schema) storing file metadata,
  bytes in **MinIO** via an `IObjectStorage` port (bucket auto-created); upload (multipart) / list /
  download / delete endpoints, permission-gated (`files.read`/`files.manage`) + audited
- ⬜ Real LGSP/NDXP connectors + XML/JSON data-exchange feeds (registry + ACL scaffolding in place)
- ⬜ Security Level-3 hardening checklist + assessment; **legacy data migration** (Doc 04 §7); go-live

### Frontend
- ✅ **Light theme is the default for all pages**; auth-gated; bearer-token interceptor
- ✅ **Edit + delete** on Org Units and Indicators (modal edit + Popconfirm delete)
- ✅ **Delete on every list page** (Sector Data ×5, Users, Roles, Catalog master data ×3) —
  EF ExecuteDelete, audited
- ✅ **Edit modals on every list page**: Org Units, Indicators, the 5 Sector entities (Clusters,
  Petroleum Stations, Commerce Locations, E-commerce, Market Violations — incl. status/sanction/fine),
  Users (incl. active toggle) & Roles, and the 3 Catalog master-data pages (Indicator Sets, Report
  Templates, Reporting Periods). Backed by per-aggregate `Update` domain methods + PUT endpoints
  (validated + audited); natural keys (code/tax/case no/username) are immutable on edit
- ✅ Catalog (grouped nav): Indicators, **Indicator Sets, Report Templates, Reporting Periods,
  Administrative Units, Classifications**
- ✅ Pages: Org Units, Users, Roles, Industrial Clusters, Observations, Market Violations,
  Petroleum Stations, Commerce Locations, E-commerce Participants (list / search / create)
- ✅ **Campaigns** + **Submissions** (workflow action buttons per state + transition-history timeline)
- ✅ **Notifications** page + header bell with unread badge
- ✅ **Dashboard** (landing page): statistic cards + **charts** (Recharts) — entity-distribution bar,
  submissions-by-state pie, violations-by-status bar
- ✅ **Map** (Leaflet/OpenStreetMap): toggleable layers for clusters, petrol stations, commerce locations
- ✅ **Audit log** page (search by user/action, expandable payload)
- ✅ **Integration** page (connection-status panel + data-sharing service registry with publish/revoke)
- ✅ **Files** page (upload to MinIO, list, download, delete)
- ✅ **Batch-import UI**: reusable `ImportModal` (drag-drop .xlsx/.xml/.csv → validated preview table
  → commit) wired into all 6 Sector pages, with downloadable CSV templates
- ✅ **Read-only detail views**: reusable `DetailDrawer` (antd `Descriptions`) + a "Xem" action on
  every main CRUD list page (6 Sector + Org Units/Users/Roles + 4 Catalog), resolving ids→names from
  loaded lookups

## Verification (current)
- `dotnet build` → 0 warnings / 0 errors; no known-vulnerable dependencies
- `dotnet test` → **62/62 pass** — 60 unit (incl. import parsers + bulk-import handler, admin-unit +
  classification domain, observation workflow, report auto-extract) + **2 integration** (Testcontainers
  PostgreSQL: outbox interceptor, ltree subtree/data-scope)
- Outbox pipeline verified at runtime: seeded `OrgUnitCreated` events written to the outbox and
  drained by the processor (`total=2, processed=2`)
- `npm run build` (frontend) → OK
- Runtime smoke test → `identity` + `catalog` + `sector` schemas migrate, dev seed applies, PostGIS
  geometry column + GIST index created, all endpoints reject anonymous callers (401)

## Next up
All 7 designed bounded contexts are built; CI/CD, integration tests, and observability are in place.
Remaining work is hardening, real integrations, and polish:
- Security **Level-3 hardening** checklist + assessment readiness; **legacy data migration** (Doc 04 §7)
- Real **LGSP/NDXP** connectors + XML/JSON data-exchange feeds
- Log aggregation (Seq/Loki) + Grafana dashboards
- UX polish: frontend code-splitting (bundle ~2 MB)
  (internal tile server for the GIS map in closed networks)

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
