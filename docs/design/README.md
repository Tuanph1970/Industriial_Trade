# Industry & Trade Sector Database (CSDL Ngành Công Thương) — System Design

> **Project:** Upgrade of the Industry & Trade Sector Database for the Department of
> Industry & Trade (Sở Công Thương) of the merged Hưng Yên province (Hưng Yên + Thái Bình),
> operating under the 2-tier government model (Province → Commune/Ward).
>
> **Status:** Design baseline · **Language:** English · **Date:** 2026

This folder contains the full technical design and implementation plan, derived from the
Vietnamese technical requirements document (`../Mo ta ky thuat BC nganh CT.pdf`). The original
document's architecture (3-tier SOA, .NET Framework 4.8 / SQL Server / Windows Server) was
**not** carried over — per the client's instruction we focused on the *required features* and
designed a better, modern architecture.

## Document set

| # | Document | Contents |
|---|----------|----------|
| 1 | [01-requirements-analysis.md](./01-requirements-analysis.md) | Problem analysis, scope, actors, use cases, business processes, non-functional & compliance requirements (translated & restructured) |
| 2 | [02-solution-architecture.md](./02-solution-architecture.md) | Architecture style, technology stack, deployment topology, cross-cutting concerns, design patterns catalogue |
| 3 | [03-domain-model-ddd.md](./03-domain-model-ddd.md) | Bounded contexts, aggregates, domain events, the reporting-approval state machine/saga |
| 4 | [04-data-architecture.md](./04-data-architecture.md) | PostgreSQL schema strategy, partitioning, PostGIS, audit, scaling, data migration from the legacy SQL Server |
| 5 | [05-implementation-plan.md](./05-implementation-plan.md) | Phased delivery plan, milestones, team mapping, backlog, risks, definition of done |

## Key decisions (ADR summary)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Architecture style | **DDD modular monolith → extractable services** | Matches 1–2 server / Docker Compose reality; avoids premature microservices overhead; clean bounded contexts keep the split-later path open |
| Backend platform | **.NET 10 (LTS, support → Nov 2028)**, C#, ASP.NET Core | Latest LTS; runs on Linux/containers; team continuity with the existing C# codebase |
| Frontend | **React + TypeScript (Vite)** SPA on Node.js, Ant Design, MapLibre/Leaflet | Data-heavy government admin UI; rich tables/forms; mapping for clusters & petrol stations |
| Database | **PostgreSQL 16 + PostGIS** | Superior for analytical/statistical queries, partitioning, JSON, and GIS; open-source; (SQL Server *does* run on Linux but is not the best fit for a free redesign) |
| Distributed DB? | **No** | Workload is low-millions of rows; a single partitioned node + read replica is correct. Sharding would add cost with no benefit. |
| Identity / SSO | **Keycloak (OIDC)** | Open-source IAM; centralized auth; ready for future provincial SSO |
| Async / workflow | **RabbitMQ + Outbox + Saga** | Reliable domain events; long-running report-approval process |
| Cache / files / search | **Redis · MinIO (S3) · Postgres FTS** | Self-hostable, S3-compatible object store for the file/resource module |
| Security target | **Information Security Level 3** (Decree 85/2016, Circular 12/2022) | Mandated by the requirements |

## Out of scope (confirmed with client)

- Public-facing portals (online public services / DVC, e-commerce promotion ecosystem, public
  planning-map lookup). The system is **internal**: Department staff + commune officials +
  gov-to-gov API integration via LGSP/NDXP.
- AI / Big-Data forecasting — treated as a *future* extension point, not a current deliverable.
