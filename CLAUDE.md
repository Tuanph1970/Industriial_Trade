# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project status: walking skeleton scaffolded

Two bounded contexts are built end-to-end (DB → API → UI): **IdentityAccess** (org units, users,
roles, Keycloak auth + DB-driven function/data-scope authorization) and **Catalog** (versioned
statistical indicators). Five contexts remain (SectorData, Reporting, Analytics, Integration,
AuditSystem) — replicate the IdentityAccess/Catalog shape. The frontend uses an explicit **light
theme** (`theme.defaultAlgorithm`) as the default for all pages.

Authorization note: function-scope permissions and data-scope org-unit paths are resolved from the
**IdentityAccess database** (a user's roles + assigned unit) via a claims transformation after
Keycloak authentication — not from static token attributes. A dev seeder creates demo org units,
roles, and users (`superadmin`, `chuyenvien`) matching the Keycloak realm.

- `docs/Mo ta ky thuat BC nganh CT.pdf` — authoritative Vietnamese requirements. 75 pages; read with
  the `pages` parameter in chunks.
- `docs/design/` — English design baseline (read `docs/design/README.md` first; ADR-style decisions).
- `backend/` — .NET 10 solution (`IndustryTrade.slnx`). See `backend/README.md`.
- `frontend/` — React + TS + Vite + Ant Design SPA.
- `deploy/` — Docker Compose infra (Postgres/PostGIS, Redis, RabbitMQ, MinIO, Keycloak) + Dockerfiles.

### Commands

⚠️ **Use the local .NET 10 SDK** — the system `dotnet` is 9.x (EOL) and cannot build `net10.0`:
```bash
export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="$HOME/.dotnet"
```
```bash
# backend (from backend/)
dotnet build                                   # build solution
dotnet test                                    # all tests
dotnet test --filter "FullyQualifiedName~OrgUnitTests"   # single test class
dotnet run --project src/Hosts/IndustryTrade.Api         # API + Swagger; auto-migrates in Dev
dotnet dotnet-ef migrations add <Name> -p <Module>.Infrastructure -s src/Hosts/IndustryTrade.Api -o Persistence/Migrations

# frontend (from frontend/)
npm install && npm run dev                     # dev server :5173, proxies /api to :8080
npm run build                                  # tsc typecheck + vite build

# full stack (from deploy/)
cp .env.example .env && docker compose up -d --build   # api :8080, frontend :8081, keycloak :8090
```

To add a bounded context, copy the IdentityAccess four-project shape and register it in
`Program.cs` — full steps in `backend/README.md`.

## What the system is

Upgrade of the **Industry & Trade Sector Database (CSDL ngành Công Thương)** for the Department of
Industry & Trade of the merged **Hưng Yên province** (Hưng Yên + Thái Bình), under Vietnam's 2-tier
government model. It captures statistical indicators (industry, energy, commerce, market
surveillance), runs a commune→department report **approval workflow**, provides leadership analytics,
and integrates gov-to-gov via LGSP/NDXP. **Internal-use only** (no public portal). Must meet
**Information Security Level 3** and align with Vietnam Digital Government Architecture Framework v4.0.

## Architecture decisions that span multiple documents

These were deliberately chosen against the original PDF's stack (3-tier SOA / .NET Framework 4.8 /
SQL Server / Windows) — the client asked to ignore that and design better. Preserve these unless told
otherwise:

- **DDD modular monolith, not microservices.** Seven bounded contexts (Identity&Access, Catalog,
  SectorData, Reporting&Workflow, Analytics, Integration, Audit&System) are separate
  modules/assemblies but deploy as **a few Docker-Compose containers** on 1–2 Linux servers — *not*
  Kubernetes. Boundaries are strict (in-process MediatR + domain events, no cross-context table
  access) so any context can be extracted into its own service later. Don't add fine-grained services
  or k8s without an explicit reason. See `docs/design/02` and `03`.
- **Stack:** .NET 10 (LTS) / ASP.NET Core backend; React + TypeScript (Vite) + Ant Design frontend on
  Node; PostgreSQL 16 + **PostGIS** (chosen over SQL Server/MySQL for analytical + GIS workload);
  Keycloak (OIDC), Redis, RabbitMQ + MassTransit, MinIO, Nginx. Everything containerized.
- **No distributed/sharded database.** Single partitioned primary + read replica + materialized
  views. The data is low-millions of rows, not big data.
- **The recurring-change defense (most important data decision):** statistical indicators change with
  each ministry circular, so the model uses a **versioned indicator catalog** + a generic, partitioned
  `IndicatorObservation` table for numeric statistics. Rich domain entities (industrial clusters,
  petroleum stations, commerce locations, market-violation cases) exist *only* where there's real
  behavior/geometry. **New indicators = new catalog rows, never a schema migration.** See
  `docs/design/03 §4` and `04 §3`.
- **Two-dimensional authorization** is a hard requirement, not an afterthought: *function-scope*
  (policy-based, mapped to Keycloak roles) AND *data-scope* (row filtering by the org-unit tree via
  `ltree`/`DataScopeResolver`, applied as EF global query filters). See `docs/design/02 §5`, `03 §2`.
- **The report approval workflow** (commune official → specialist → division leader → director) is the
  core process, modeled as a **State machine + Saga** with Outbox-backed notifications. See
  `docs/design/03 §5`.

## Conventions to follow when coding starts

- Per-module Clean/Onion layering: `Module.Api → Module.Application → Module.Domain`;
  `Module.Infrastructure` implements ports. Domain depends on nothing.
- Cross-cutting via MediatR pipeline behaviors (validation, logging, transaction, authorization,
  audit). The **Specification pattern** backs the pervasive list/search/paginate use cases — reuse it
  rather than hand-writing per-entity query code.
- One PostgreSQL database, **schema-per-bounded-context** (`identity`, `catalog`, `sector`,
  `reporting`, `analytics`, `audit`, `integration`); no cross-schema foreign keys.
- All documentation is written in **English**; the domain/UI is Vietnamese (Unicode, TCVN 6909:2001).
  A vi→en ubiquitous-language glossary is in `docs/design/03 §9` — use those English model terms in
  code.

## Note on the surrounding directory

The parent folder (`/home/tuanph/TuanPH`) is a home directory holding several unrelated projects.
This repo (`Industriial_Trade`, git remote `github.com/Tuanph1970/Industriial_Trade`) is
self-contained — scope work to it.
