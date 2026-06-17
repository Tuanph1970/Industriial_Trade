# 03 — Domain Model (DDD)

## 1. Bounded contexts & context map

Seven bounded contexts. Arrows show dependency direction (downstream depends on upstream contract);
all cross-context calls go through published contracts + domain events, never shared tables.

```
                ┌────────────────────┐
                │ Identity & Access   │◀─────────── (everyone consumes principal/org-scope)
                │  (Upstream/Shared)  │
                └─────────┬───────────┘
                          │ org units, principals, data-scope
        ┌─────────────────┼───────────────────────────┐
        ▼                 ▼                             ▼
┌───────────────┐ ┌─────────────────┐          ┌──────────────────┐
│ Catalog /     │ │ Sector Data      │          │ Audit & System   │
│ Master Data   │─▶│ (Industry,      │          │ (logs, files,    │
│ (indicators,  │ │  Energy,         │          │  backup meta)    │
│  templates,   │ │  Commerce,       │          └──────────────────┘
│  periods)     │ │  Market Surv.)   │
└──────┬────────┘ └───────┬──────────┘
       │ indicator defs   │ observations / records
       ▼                  ▼
┌──────────────────────────────────┐      ┌──────────────────────┐
│ Reporting & Workflow              │─────▶│ Analytics / Dashboards│
│ (periods, submissions, approval   │ read │ (read models, charts) │
│  saga, notifications)             │ model└──────────────────────┘
└──────────────┬───────────────────┘
               │ outbound/inbound data exchange
               ▼
        ┌──────────────────────┐
        │ Integration (ACL)     │──▶ LGSP / NDXP-NGSP, external gov systems
        └──────────────────────┘
```

**Relationship types:** Identity & Access and Catalog are **upstream/shared kernel-ish** suppliers;
Sector Data and Reporting are **customers** of Catalog (conformist to indicator definitions);
Integration is an **Anti-Corruption Layer** isolating external schemas; Analytics is a pure
**downstream read model** (CQRS) fed by domain events.

## 2. Identity & Access context

**Aggregates**
- **Organization (Org Unit)** — *aggregate root*. Multi-level tree (parent ↔ children). Holds code,
  name, type (Department / Division / Commune), parent reference, optional geolocation. Invariant: no
  cycles; a unit's data-scope encloses its descendants.
- **UserAccount** — root. Identity-provider subject id (Keycloak), profile, assigned org unit(s),
  status. Behavior: `ResetPasswordToDefault()`, `AssignToUnit()`, `Activate/Deactivate()`.
- **Role** — root. Named bundle of **function permissions**.
- **PermissionGrant** — value within Role/User: (permission code, scope). Two scope kinds:
  `FunctionScope` and `DataScope(orgUnitId, includeDescendants)`.

**Domain services:** `DataScopeResolver` (principal → set of accessible org-unit ids),
`PermissionEvaluator`.

**Key domain events:** `UserCreated`, `UserDeactivated`, `PasswordReset`, `OrgUnitMoved`,
`RoleAssigned`.

## 3. Catalog / Master Data context

**Aggregates**
- **Indicator** — root. Definition of a statistical indicator per Circular 33/2022: code, name,
  unit-of-measure, data type (number/text/enum), calculation rule, sector, validity period
  (effective/retired — supports add/modify/remove of indicators across circular versions).
- **IndicatorSet (Bộ chỉ tiêu)** — root. Ordered grouping of indicators for a reporting context.
- **ReportTemplate** — root. Structure of a statistical report form (per Circular 34): sections,
  rows/columns, bound indicators, document-form layout.
- **ReportingPeriodDefinition** — root. Period type (month/quarter/year), schedule, deadlines.
- **AdministrativeUnitCatalog** — commune/ward list of the merged province (reference data).
- Supporting catalogs: statistical fields, classification/grouping catalogs.

**Why a catalog context:** the new indicator system *changes over time* (circular revisions). Keeping
indicator/template definitions versioned and separate from the captured data lets Sector Data and
Reporting evolve without schema churn — a direct answer to the "data structure outdated" problem.

**Events:** `IndicatorPublished`, `IndicatorRetired`, `TemplateVersioned`, `ReportingPeriodOpened`.

## 4. Sector Data context

This is the largest context (UC-10…29). Design insight: the use cases split into **two shapes**, so
we model them with two complementary patterns instead of 20 near-duplicate CRUD tables.

### 4a. Rich domain entities (have identity, attributes, lifecycle, location)
Modeled as first-class aggregates with their own tables and behavior:
- **Enterprise / ProductionFacility**
- **IndustrialCluster** (with PostGIS geometry)
- **PetroleumStation** (geometry, license info)
- **EcommerceParticipant** (business + goods on platforms)
- **CommerceLocation** (market / supermarket / mall / convenience store; geometry)
- **MarketViolationCase** — root for D1/D2 violation records (group: prohibited/counterfeit, or
  food-safety); holds inspection, parties, sanctions, attachments. (This is "Quản lý hồ sơ".)

### 4b. Statistical observations (numeric values of an indicator for a unit & period)
Modeled as a generic, partitioned **IndicatorObservation** aggregate:
`(indicatorId, orgUnitId, periodId, value, source, status, capturedBy)`.
This single well-indexed, partitioned structure serves *all* numeric sector indicators
(mining, heavy/manufacturing industry, energy, market-surveillance counts, etc.) — driven by the
Catalog's indicator definitions rather than bespoke columns. New indicators = new catalog rows, **no
schema migration**.

> Rich entities (4a) carry the *who/what/where* master data; observations (4b) carry the *how-much
> over time* statistics that link to them and to indicator definitions. Reports and analytics read
> both.

**Shared building blocks:** the **Specification pattern** provides reusable, composable filters
(by sector, unit, period, keyword, status) behind every list/search/paginate use case; **Strategy**
handles per-format import (Excel/XML/manual) and per-indicator computation.

**Events:** `ObservationRecorded`, `ObservationCorrected`, `ViolationCaseFiled`,
`ClusterRegistered`, etc.

## 5. Reporting & Workflow context (the heart of the system)

Orchestrates Processes 3.1 and 3.2. Modeled as a **Saga / Process Manager** driving a report
aggregate whose lifecycle is a **State machine**.

**Aggregates**
- **ReportingCampaign** — opened by a specialist for a `ReportingPeriodDefinition`; tracks which
  org units must submit; emits notifications.
- **ReportSubmission** — root; the unit-of-work that moves through the approval workflow. Holds the
  filled template data (or links to IndicatorObservations), attachments, current state, history.

**State machine (ReportSubmission):**

```
        create/extract            submit              accept-review        approve
 (none) ───────────▶ Draft ─────────────▶ Submitted ───────────▶ UnderReview ──────────▶ PendingApproval
                       ▲                     │                        │                        │
                       │ reject (commune)    │ return (specialist)    │ reject (specialist)    │ approve
                       └─────────────────────┴────────────────────────┴────────────┐           ▼
                                                                                    └──────▶ Approved
                                          leader rejects ──────────────────────────────────▶ Rejected ──▶ (back to Draft)
```

Transitions raise domain events consumed by the Saga to send notifications (to commune official,
division leader, deputy director, director) and to update the Analytics read model. Guard rules:
only the holder of the right *function* permission **and** *data-scope* for the unit may trigger a
transition (enforced via the authorization pipeline + the aggregate's own invariants).

**Patterns:** State (lifecycle), Saga/Process Manager (coordination + notifications + retries),
Specification (worklist queries: "reports awaiting my review"), Outbox (reliable notifications).

**Events:** `CampaignOpened`, `ReportSubmitted`, `ReportReturned`, `ReportApproved`,
`ReportRejected`, `ReportPublished`.

## 6. Analytics / Dashboards context (read side of CQRS)

Pure **read models** built by projecting domain events from Sector Data and Reporting into
denormalized tables / **materialized views** optimized for the aggregate reports (Industry,
Commerce, Market-management) and leadership dashboards/charts. Refreshed on events and on schedule.
This is where the "reduce report-aggregation time by ~30%" NFR is delivered — pre-aggregated views
instead of on-the-fly joins over the operational store.

## 7. Audit & System context

- **AuditLogEntry** (append-only, monthly-partitioned): actor, action, target, before/after summary,
  timestamp, IP. Fed by the audit pipeline behavior; searchable by many criteria (requirement G1).
- **ManagedFile** — root for the file/resource module (UC-4); stored in MinIO, metadata + access
  control in Postgres; fixes the legacy security holes (virus scan hook, signed URLs, scoped access).
- **BackupSession** — metadata for backup/restore operations and data-cleaning runs (requirement G2).

## 8. Integration context (Anti-Corruption Layer)

- **OutboundDataService** / **InboundDataService** — adapters translating between internal aggregates
  and the XML/JSON exchange schemas required by LGSP/NDXP (Decree 47/2020, Circular 13/2017).
- **ConnectionStatus** — aggregates component health for the status-reporting REST API (requirement
  F2); persists status history ≥ 3 months.
- **ServiceRegistration** — tracks published/consumed data-sharing services and their lifecycle.

The ACL guarantees external schema changes never leak into the core domain.

## 9. Ubiquitous-language glossary (vi → en)

| Vietnamese | English (model term) |
|------------|----------------------|
| Cơ quan, đơn vị | Organization / Org Unit |
| Chỉ tiêu (thống kê) | Indicator |
| Bộ chỉ tiêu | Indicator Set |
| Kỳ báo cáo | Reporting Period / Campaign |
| Biểu mẫu báo cáo | Report Template |
| Báo cáo (điện tử) | Report Submission |
| Cụm công nghiệp | Industrial Cluster |
| Cửa hàng xăng dầu | Petroleum Station |
| Hồ sơ vi phạm | Market Violation Case |
| Phê duyệt / Duyệt | Approve / Review |
| Cán bộ cơ sở | Commune Official |
| Chuyên viên | Specialist |
| Lãnh đạo phòng | Division Leader |
| Nhật ký (quản trị) | Audit Log |
| Phân quyền dữ liệu / chức năng | Data-scope / Function-scope permission |
