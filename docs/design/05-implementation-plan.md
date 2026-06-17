# 05 — Implementation Plan

## 1. Approach

- **Iterative, vertical slices.** Deliver end-to-end (DB → API → UI) per bounded context so each
  phase produces something demonstrable and testable, rather than horizontal layers that integrate
  only at the end.
- **Walking skeleton first.** Stand up the full pipeline (auth, one module, CI/CD, containers,
  observability) in Phase 0/1 so risk is retired early.
- **Foundation contexts before dependent ones:** Identity & Catalog → Sector Data → Reporting →
  Analytics → Integration → hardening/migration/go-live.
- Source says "within 2026, no phasing." A realistic engineering schedule is **~9–11 months** of
  build + UAT + go-live; below is sprint-based (2-week sprints).

## 2. Team (mapped to the source's required personnel)

| Source role | Count | Plan responsibility |
|-------------|------:|---------------------|
| Project Manager | 1 | Delivery, scope, stakeholder/Department liaison, risk |
| System Analyst / Architect | 1 | DDD modeling, architecture, API contracts, reviews |
| Software Developers | 5 | 2 backend-heavy, 2 frontend, 1 full-stack/integration |
| QA / Tester | 1 | Test strategy, automation, UAT support, security test liaison |
| Trainer (transfer) | 1 | Docs, training materials, user training |
| Deployment Engineer | 1 | Infra, Docker Compose, CI/CD, security level-3 setup, migration ops |

## 3. Phases & milestones

### Phase 0 — Inception & Foundations (Sprints 1–2, ~4 wks)
- Environments: dev/staging on Ubuntu 22.04/24.04 + Docker Compose; Git repo, branching, CI/CD.
- Walking skeleton: solution structure (modular monolith + Worker), one trivial module end-to-end,
  PostgreSQL+PostGIS, Keycloak, Redis, RabbitMQ, MinIO, Nginx, Serilog/OpenTelemetry.
- Cross-cutting libraries: MediatR pipeline (validation/logging/transaction/authz/audit), Result type,
  Specification base, Outbox, EF conventions.
- **Milestone M0:** "Hello, secured, observable, containerized vertical slice" deployed to staging.

### Phase 1 — Identity, Organization & Access (Sprints 3–4)
- Org-unit tree (ltree) + UI; users; roles; **two-dimensional authorization** (function-scope policies
  + data-scope filter); Keycloak integration; password reset; audit logging live.
- **M1:** Admins can manage units/users/permissions; every action audited.

### Phase 2 — Catalog / Master Data (Sprints 5–6)
- Indicator catalog (versioned, Circular-33 aligned), indicator sets, report templates (Circular-34),
  reporting-period definitions, administrative-unit & classification catalogs; batch import skeleton.
- **M2:** New indicator/reporting structures fully manageable; ready to receive data.

### Phase 3 — Sector Data (Sprints 7–10)
- Generic `IndicatorObservation` (partitioned) + Specification-based list/search/paginate reused
  across all numeric indicators.
- Rich entities: clusters, petrol stations, commerce locations, e-commerce participants, enterprises,
  **market-violation cases** (D1/D2) — with PostGIS + map UI.
- Excel/XML import (Strategy pattern), validation, detail views.
- **M3:** All UC-10…29 manage/search/detail features live for Industry, Energy, Commerce, Market.

### Phase 4 — Reporting & Approval Workflow (Sprints 11–13)
- ReportingCampaign + ReportSubmission **state machine** + **Saga**; commune→specialist→leader
  approval chain; notifications; worklists ("awaiting my review"); auto-extract reports from
  observations per template.
- **M4:** Processes 3.1 & 3.2 work end-to-end with real roles.

### Phase 5 — Analytics & Dashboards (Sprints 14–15)
- CQRS read models / materialized views; aggregate reports (Industry, Commerce, Market-management);
  leadership dashboards & charts; export.
- **M5:** Leadership dashboards live; aggregation meets the ~30%-faster target.

### Phase 6 — Integration, Security Hardening & Go-Live (Sprints 16–19)
- Integration ACL: LGSP/NDXP connectors, XML/JSON exchange, **connection-status API** (≥3-month
  history), service registration (Decree 47/2020).
- **Security Level 3** hardening + assessment readiness (TLS, secrets, scans, pen-test fixes,
  backup/restore drills).
- **Legacy data migration** dry-runs → final cutover (Doc 04 §7).
- UAT, training, documentation handover; production go-live + warranty/hypercare.
- **M6:** Production live, integrated, secured, staff trained, data migrated.

## 4. Indicative timeline

```
Month:   1   2   3   4   5   6   7   8   9   10  11
P0  ▓▓
P1      ▓▓▓▓
P2          ▓▓▓▓
P3              ▓▓▓▓▓▓▓▓
P4                      ▓▓▓▓▓▓
P5                            ▓▓▓▓
P6                                ▓▓▓▓▓▓▓▓ (incl. UAT, migration, go-live)
```
Phases overlap where safe (e.g. frontend scaffolding during Phase 1; integration spikes early).

## 5. Cross-cutting workstreams (run throughout)
- **Testing:** unit (domain), integration (Testcontainers: Postgres/RabbitMQ/Redis/MinIO), API
  contract tests, E2E (Playwright), load tests before go-live. Target meaningful domain coverage,
  not a vanity %.
- **Security:** threat modeling, dependency/image scanning in CI (Trivy, `dotnet`/`npm audit`),
  Level-3 control checklist tracked from Phase 0.
- **DevOps:** CI (build/test/scan) + CD (image → staging → prod via Compose) from day one; DB
  migrations gated and reversible.
- **Docs & training:** living architecture docs (this folder), API (OpenAPI), runbooks, user manuals,
  admin guide; train-the-trainer for Department & commune staff.

## 6. Definition of Done (per feature)
Code reviewed · automated tests green · validation + authorization (both dimensions) enforced ·
audited · OpenAPI updated · UI accessible & Vietnamese/Unicode-correct · deployed to staging · demoed
& accepted.

## 7. Risk register

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Indicator/template churn (circular revisions) | Med | Versioned catalog + generic observations → no schema change for new indicators |
| Microservices over-engineering on small infra | Med | Modular monolith now, extract-later boundaries; no premature k8s |
| Legacy data quality / mapping gaps | High | Early profiling, staging dry-runs, specialist sign-off, archive retired data |
| Ubuntu 20.04 EOL | Med | Containerize; require 22.04/24.04 hosts |
| LGSP/NDXP integration dependencies (external) | Med | ACL isolation, start integration spikes early, mock until endpoints ready |
| Security Level-3 assessment delays go-live | High | Build controls from Phase 0; pre-assessment before cutover |
| Adoption by commune officials | Med | Simple UX, batch import, list pickers, training, hypercare |

## 8. Immediate next steps
1. Approve this design baseline (Docs 01–05).
2. Provision dev/staging Linux hosts (22.04/24.04) + Docker.
3. Initialize the repository & solution skeleton (modular monolith + Worker + Compose).
4. Stand up the Phase-0 walking skeleton (auth + one module + CI/CD + observability).
5. Obtain legacy SQL Server access for early data profiling.
