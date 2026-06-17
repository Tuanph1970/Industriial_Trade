# 04 — Data Architecture

## 1. Principles

- **One logical database, schema-per-bounded-context.** PostgreSQL schemas (`identity`, `catalog`,
  `sector`, `reporting`, `analytics`, `audit`, `integration`) enforce module boundaries while keeping
  a single node to operate. Each module owns its schema; no cross-schema foreign keys — cross-context
  links are by id + validated through contracts. This preserves the "extract to a service later" path
  (a schema can be moved to its own database).
- **No distributed/sharded database.** Volume is low-millions of rows; a single partitioned primary +
  read replica is the right tool. Revisit only if data or load grows by orders of magnitude.
- **Partition the hot, append-heavy tables**; keep master/reference data small and cached.
- **PostGIS** for all spatial attributes (clusters, petrol stations, commerce locations).

## 2. Why PostgreSQL + PostGIS (vs SQL Server / MySQL)

| Need | PostgreSQL fit |
|------|----------------|
| Complex statistical aggregation, window functions, CTEs | Excellent |
| Time/sector **table partitioning** (declarative) | Native, mature |
| **GIS** (maps for clusters, petrol stations, markets) | **PostGIS** — best-in-class |
| Flexible/evolving indicator attributes | `JSONB` + GIN indexes |
| Full-text search (the pervasive keyword filters) | Built-in FTS + `pg_trgm` |
| Cost / licensing (government, open-source mandate) | Free, no per-core license |
| Read scaling for reporting | Streaming replication (read replica) |

SQL Server runs on Linux too, but PostgreSQL is the stronger fit for this analytical + spatial
workload and aligns with the open-source choice.

## 3. Key schema designs

### 3.1 Organization tree (`identity.org_unit`)
Multi-level parent/child (fixes the legacy "no multi-level units" defect). Use **`ltree`** (or a
closure table) for efficient subtree/data-scope queries:

```
org_unit(id, code, name, type, parent_id, path ltree, geom geography(Point,4326),
         status, created_at, ...)
-- GIST index on path → fast "all descendants of unit X" (data-scope resolution)
CREATE INDEX ix_org_path ON identity.org_unit USING gist (path);
```

`DataScopeResolver` turns a principal's granted unit(s) into a `path <@ ANY(...)` predicate reused as
an EF global query filter across contexts.

### 3.2 Indicator catalog (`catalog.indicator`) — versioned
```
indicator(id, code, name, uom, data_type, sector, calc_rule jsonb,
          effective_from, retired_at, version, ...)
```
Versioning + `retired_at` supports add/modify/remove of indicators between circular revisions
**without breaking historical data**.

### 3.3 Statistical observations (`sector.indicator_observation`) — partitioned
The generic numeric-value table behind most sector indicators:
```
indicator_observation(
   id, indicator_id, org_unit_id, period_id,
   value numeric, value_text text, attributes jsonb,
   source, status, captured_by, recorded_at
) PARTITION BY RANGE (period_year);   -- yearly partitions
-- sub-partition or composite index by sector if needed
CREATE INDEX ix_obs_lookup ON sector.indicator_observation
   (indicator_id, org_unit_id, period_id);
```
Yearly partitions keep each partition small, make purges/rollovers cheap, and speed period-scoped
reports. New indicators never require DDL — only new `catalog.indicator` rows.

### 3.4 Rich entities (master data with geometry)
```
sector.industrial_cluster(id, code, name, org_unit_id, area_ha, status,
                          geom geometry(MultiPolygon,4326), attributes jsonb, ...)
sector.petroleum_station(id, code, name, org_unit_id, license_no,
                          geom geometry(Point,4326), ...)
sector.commerce_location(id, type, name, org_unit_id, geom geometry(Point,4326), ...)
sector.market_violation_case(id, case_no, group, org_unit_id, inspected_at,
                          parties jsonb, sanctions jsonb, status, ...)
-- GIST spatial indexes on every geom column
```

### 3.5 Reporting & workflow
```
reporting.campaign(id, period_def_id, opened_by, opened_at, deadline, status, ...)
reporting.report_submission(id, campaign_id, org_unit_id, template_version_id,
                            state, payload jsonb, current_holder, ...)
reporting.report_history(id, submission_id, from_state, to_state, actor, at, note)
```
`state` + `report_history` realize the State machine and give a full approval audit trail.

### 3.6 Audit (`audit.log_entry`) — partitioned monthly, append-only
```
audit.log_entry(id, actor_id, action, target_type, target_id,
                before jsonb, after jsonb, ip, at) PARTITION BY RANGE (at);
```
Searchable by actor/action/target/date; delete operations always recorded (requirement G1).

### 3.7 Outbox (`<schema>.outbox_message`)
Per-context outbox table written in the same transaction as aggregate changes; drained by the Worker
→ RabbitMQ. Guarantees no lost domain events / notifications.

## 4. Read models / analytics (CQRS read side)
`analytics.*` **materialized views** and projection tables (e.g. `mv_industry_summary`,
`mv_commerce_summary`, `mv_market_mgmt_summary`) refreshed on events + schedule. Dashboards/aggregate
reports query these, not the operational tables → delivers the ~30% faster aggregation NFR.

## 5. Scaling & availability (without distribution)
- **Read replica** for reporting/analytics & backups; primary handles writes.
- **Partition pruning** + targeted indexes for query performance.
- **Redis** caches catalogs/reference data.
- **PgBouncer** connection pooling.
- Vertical scaling on the modest servers is ample for the expected load; horizontal read scaling via
  additional replicas if needed.

## 6. Backup, restore & data hygiene (requirements 4.1, G2)
- **Physical:** `pgBackRest` — full + incremental, encrypted, off-host retention; PITR via WAL.
- **Logical:** scheduled `pg_dump` per schema for granular restore + migration safety.
- **Restore drills** scheduled and documented; restore time objective tracked.
- **Data-cleaning/consistency jobs** (Worker + Quartz) with historical snapshots kept for rollback.
- All backup/restore/cleaning runs recorded in `audit` + `BackupSession` metadata.

## 7. Legacy data migration (from SQL Server 2019)
A one-off, repeatable **ETL pipeline** (its own console/worker project):
1. **Profile** the legacy SQL Server schema & data quality.
2. **Map** legacy tables → new bounded-context schemas; map old indicators → new
   Circular-33 indicator catalog (record removed/modified/added indicators).
3. **Extract** (SQL Server connector) → **Transform** (normalize, dedupe, geocode where possible,
   validate against catalog) → **Load** (staging schema → validated → production).
4. **Reconcile**: row counts, checksums, sample audits signed off by domain specialists.
5. **Dry-run repeatedly** in staging; final cutover during the go-live window with a rollback plan.
Removed-indicator data is archived (not silently dropped) per the source's "backup & remove retired
modules" instruction.
