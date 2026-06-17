# 01 — Requirements Analysis

This document restates the requirements from the Vietnamese source (`Mo ta ky thuat BC nganh
CT.pdf`) in English, restructured for clean software design. Use-case numbering follows the
source where helpful, but functions are regrouped into logical domains.

## 1. Background & Problem Statement

- Hưng Yên and Thái Bình provinces were **merged** into a new, larger Hưng Yên province.
- Government moved to a **2-tier model**: Province/City → Commune/Ward (the district tier was
  removed). Data and operations must be centralized accordingly.
- The existing sector database (built 2021: ASP.NET Core 3.1, SQL Server 2019, Windows Server,
  3-tier/SOA) is now inadequate:
  - Data structures do not match the **new statistical indicator system** —
    *Circular 33/2022/TT-BCT* (indicator system) and *Circular 34/2022/TT-BCT* (reporting regime),
    which replaced Circulars 40/2016 and 41/2016.
  - Only covers former Hưng Yên scope; cannot serve the merged province.
  - Weak extensibility, integration, and data exploitation; org structure cannot represent
    multi-level units; permission model too limited; file/resource module has security holes;
    account/password management is awkward.

**Goal:** Re-platform and extend the system to (a) unify data across Industry, Energy, Commerce,
and Market-surveillance domains for the merged province, (b) implement the new indicator/reporting
system, (c) support the commune→department reporting & approval workflow, (d) provide analytics for
leadership, and (e) integrate with provincial/national platforms — all at Information Security
Level 3.

## 2. Actors / Roles

| # | Actor | Description |
|---|-------|-------------|
| 1 | **System Administrator** | Highest technical privilege; manages the whole system. |
| 2 | **Director (Giám đốc Sở)** | Department head; admin rights + aggregate-report views. |
| 3 | **Deputy Director (Phó Giám đốc)** | Admin rights + aggregate-report views. |
| 4 | **Division Leader (Lãnh đạo phòng)** | Manages data within their division's scope; approves reports. |
| 5 | **Specialist (Chuyên viên)** | Performs domain data management within delegated scope; reviews reports. |
| 6 | **Commune Official (Cán bộ cơ sở)** | At commune level; enters/submits source statistical data. |
| 7 | **External System (API client)** | Other government systems integrating via API. |

Authorization has **two independent dimensions** (a core requirement):
- **Function-scope** — which features a user/role may use.
- **Data-scope** — which organizational units' data a user/role may see/modify.

## 3. Functional Requirements (regrouped into domains)

### A. Identity, Organization & Access (Quản lý hành chính)
- **A1. Organization/Unit management** — multi-level org tree (parent unit → child units), CRUD,
  search, pagination. *(Source UC-1; current system's inability to model multi-level units is an
  explicit defect to fix.)*
- **A2. User account management** — CRUD, search, pagination; **reset password to default**;
  simplified account provisioning. *(UC-2)*
- **A3. Access-permission management** — manage function permissions and data permissions; assign
  to roles/units/users. *(UC-3, plus the data-/function-authorization modules from the system
  overview.)*
- **A4. Server file & resource management** — manage uploaded files/resources, with the security
  vulnerabilities of the old module fixed. *(UC-4)*

### B. Master Data / Catalogs (Quản lý danh mục)
- **B1. Statistical indicator catalog** (per new criteria). *(UC-5)*
- **B2. Indicator set (bộ chỉ tiêu) management.** *(UC-6)*
- **B3. Report-template catalog.** *(UC-7)*
- **B4. Statistical report templates (document/text form).** *(UC-8)*
- Reporting-period (kỳ báo cáo) catalog, statistical-field/grouping catalogs, administrative-unit
  catalog (commune list), backup-session catalog. *(Modules 9–13 in the source.)*

### C. Sector Indicator Data (Quản lý chỉ tiêu ngành)
Each of the following supports list/search/paginate, configurable page size (10/20/50/all),
create, edit, delete (with confirmation + delete audit), and a detail view:
- **C1. Mining & heavy industry indicators.** *(UC-10/20)*
- **C2. Manufacturing & processing industry indicators.** *(UC-11/21)*
- **C3. Energy/electricity indicators** — national grid, solar, wind. *(UC-12/22)*
- **C4. Commerce — business locations** — markets, supermarkets, malls, convenience stores. *(UC-13/23)*
- **C5. Commerce — petroleum stations.** *(UC-14/24)*
- **C6. E-commerce indicators** — businesses & goods on e-commerce platforms. *(UC-15/25)*
- **C7. Industrial-cluster statistics.** *(UC-16/26)*
- **C8. Market inspection/monitoring indicators** — prohibited, smuggled, counterfeit, fake,
  poor-quality goods; hoarding; market manipulation. *(UC-17/27)*

### D. Case/Record Management (Quản lý hồ sơ)
- **D1. Business-violation records** — prohibited/smuggled/counterfeit/fake/poor-quality group. *(UC-18/28)*
- **D2. Business-violation records** — hygiene & food-safety group. *(UC-19/29)*

### E. Reporting & Approval Workflow + Analytics (Báo cáo thống kê)
- **E1. Create source data from commune level** — enter/import (Excel/XML) → validate → edit →
  submit draft → specialist receives → review → division-leader approves → notify deputy/director.
  *(Process 3.1)*
- **E2. Periodic statistical reporting** — specialist creates a reporting period → notifies units →
  commune official creates/auto-extracts per template → submits → specialist review → leader approve
  → notify. *(Process 3.2)*
- **E3. Online report management / sending / approval** — the three "new" modules (14/15/16):
  manage online reports at the Department; commune officials send reports online; Department staff
  approve submitted reports online.
- **E4. Aggregate views & dashboards** — view detail per indicator; aggregate statistical reports
  for Industry, Commerce, Market-management; summary report tables & charts for leadership. *(UC-30–32 + chart module.)*

### F. Integration (gov-to-gov)
- **F1. REST APIs** providing/consuming data in XML/JSON to/from other systems via the provincial
  **LGSP** and national **NDXP/NGSP** (DVC, one-stop, document-management, IOC, shared databases).
- **F2. Connection-status service** — REST endpoint reporting health/connectivity of DB, cache,
  dependent systems (Level 1 system status, Level 2 service status); status history retained ≥ 3 months.
- **F3. Service registration/lifecycle** per Decree 47/2020 (publish, exploit, revoke data-sharing
  services).

### G. System & Audit
- **G1. Admin activity / audit log** — record all user actions; search and browse logs by multiple
  criteria; manage logs. Delete operations must be logged.
- **G2. Backup & restore management** — backup sessions, periodic data "cleaning"/consistency tools,
  historical data retention for rollback.

## 4. Business Processes (workflows)

### 4.1 Source-data creation from commune (Process 3.1)
1. Commune official enters indicators (manual or Excel/XML import).
2. System validates input; on error, restart.
3. Official edits/curates each indicator to meet data standards.
4. Official submits the electronic draft to the Department (can preview).
5. Department specialist receives the draft.
6. Specialist checks data; if inadequate, returns to commune official; else continue.
7. Specialist submits to division leader for approval.
8. Division leader approves → notification to deputy & director; or rejects → back to official.

### 4.2 Periodic statistical reporting (Process 3.2)
1. Specialist creates a new reporting period.
2. System notifies commune officials, division leaders, deputy director.
3. Commune official creates/auto-extracts the report per the Circular-34 template; validates input.
4. Official submits to the Department.
5. Specialist receives & checks; if inadequate → return; else continue.
6. Division leader reviews the electronic draft.
7. Reject → request rework by commune official.
8. Approve → notify deputy director.

Both processes share an **approval state machine** (see Doc 03 §5).

## 5. Non-Functional Requirements

| Category | Requirement |
|----------|-------------|
| **Security** | Information Security **Level 3** (Decree 85/2016, Circular 12/2022, TCVN 11930:2017). Authenticated access only; transport encryption on Internet links; centralized authentication (shared accounts with provincial systems / SSO). |
| **Data integrity** | Prevent unauthorized DB access; access only via the application; full backup/restore by several methods; periodic data-cleaning tools with historical data kept for rollback. |
| **Performance / UX** | Validate at point of entry; batch (lot) import and repeated-field entry; keyboard-only navigation, hotkeys, list pickers for fixed-value fields; reduce report-aggregation time (target ~30% faster). |
| **Internationalization** | Unicode, TCVN 6909:2001; Vietnamese diacritics; dates `DD/MM/YYYY`, 4-digit year. |
| **Compatibility** | Web-based, multi-browser (Chrome, Firefox, Cốc Cốc, Safari); **IPv6-ready**. |
| **Architecture compliance** | Vietnam Digital Government Architecture Framework v4.0 (5-layer principle), Hưng Yên provincial architecture; RESTful APIs; JSON/XML exchange; LGSP integration. |
| **Operability** | Easy to install/operate; not over-partitioned into unnecessary subsystems; stable; monitoring of service/connection status. |

## 6. Constraints & Assumptions

- **Infra:** 1–2 Linux servers, Docker Compose (no Kubernetes). Modest hardware (4–16 cores,
  16–32 GB RAM, 0.5–1 TB storage per the source).
- **Ubuntu 20.04 is EOL (Apr 2025)** — recommend Ubuntu 22.04/24.04 LTS hosts; containerization
  insulates the app from host OS.
- **Internal scope** only (see README "Out of scope").
- Timeline: implemented within 2026, no investment phasing (per source §III.4) — see Doc 05 for a
  realistic sprint schedule.
- Data volume is **not** "big data"; no distributed/sharded database.
