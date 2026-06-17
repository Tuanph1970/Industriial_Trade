# 02 — Solution Architecture

## 1. Architecture style: DDD Modular Monolith → extractable services

Given the deployment reality (1–2 Linux servers, Docker Compose) and an internal user base of a few
hundred, **fine-grained microservices would be an anti-pattern** — operational overhead, distributed
transactions, and network failure modes with no scaling benefit. Instead we use a **modular monolith
built from clean DDD bounded contexts**:

- Each bounded context is a **separate module/assembly** with its own domain model and an explicit
  public contract. Modules talk to each other **only** through these contracts (in-process MediatR
  requests + domain-event messages), never by reaching into each other's tables.
- The whole thing deploys as a **small number of containers**, not one giant process and not twenty
  tiny ones (see §4).
- Because boundaries are enforced from day one, **any context can later be extracted into its own
  service** by swapping the in-process transport for HTTP/gRPC + the message bus — without rewriting
  domain logic. This is the pragmatic "microservices-ready" path.

```
                        ┌──────────────────────────────────────────┐
   React SPA (Node) ───▶│  Nginx (TLS, reverse proxy, static)       │
   (browser, internal)  └───────────────────┬──────────────────────┘
                                             │
                        ┌────────────────────▼──────────────────────┐
                        │  API Host (ASP.NET Core 10)                │
                        │  ┌──────────────────────────────────────┐ │
                        │  │ Cross-cutting: AuthN/Z, validation,   │ │
                        │  │ logging, CQRS pipeline, outbox        │ │
                        │  └──────────────────────────────────────┘ │
                        │  Modules (bounded contexts):               │
                        │  [Identity&Access] [Catalog] [SectorData]  │
                        │  [Reporting&Workflow] [Analytics]          │
                        │  [Integration] [Audit&System]              │
                        └───┬───────────┬───────────┬──────────┬─────┘
                            │           │           │          │
                 ┌──────────▼─┐ ┌───────▼────┐ ┌────▼────┐ ┌───▼─────┐
                 │ PostgreSQL │ │  Redis     │ │RabbitMQ │ │ MinIO   │
                 │ + PostGIS  │ │  (cache)   │ │ (bus)   │ │ (files) │
                 │ + replica  │ └────────────┘ └────┬────┘ └─────────┘
                 └────────────┘                     │
                        ▲                  ┌─────────▼──────────┐
                        │                  │ Worker Host        │
                        └──────────────────│ (sagas, jobs,      │
                                           │  outbox dispatch,  │
                                           │  integration sync) │
                                           └─────────┬──────────┘
                          ┌──────────────────────────▼─────────────┐
   External gov systems ◀─┤ Keycloak (OIDC SSO)   LGSP / NDXP-NGSP │
                          └────────────────────────────────────────┘
```

## 2. Per-module internal architecture (Clean Architecture)

Every module follows the same 4-layer onion, which keeps the codebase uniform and testable:

```
Module.Api            ──▶ Endpoints/controllers, DTOs, mapping (thin)
Module.Application    ──▶ Use cases (CQRS handlers), validators, policies, ports (interfaces)
Module.Domain         ──▶ Aggregates, entities, value objects, domain events, domain services
Module.Infrastructure ──▶ EF Core repositories, external adapters, outbox, file/Redis access
```

Dependency rule: `Api → Application → Domain`; `Infrastructure → Application/Domain` (implements
ports). Domain depends on nothing. This is the standard Clean/Onion + DDD layering.

## 3. Technology stack

| Concern | Choice | Notes |
|---------|--------|-------|
| Backend runtime | **.NET 10 (LTS)** / C# / ASP.NET Core | Support to Nov 2028; Linux/container native |
| API style | REST (JSON), OpenAPI/Swagger; gRPC reserved for future inter-service | Circular-compliant JSON/XML exchange |
| CQRS / mediation | **MediatR** + pipeline behaviors | Validation, logging, transaction, authorization as decorators |
| ORM | **EF Core 10** (+ Dapper for heavy read/reporting queries) | Code-first migrations |
| Validation | **FluentValidation** | Centralized in the MediatR pipeline |
| Mapping | **Mapster** (or AutoMapper) | DTO ↔ domain |
| Frontend | **React 18 + TypeScript + Vite**, **Ant Design**, TanStack Query, React Router | Data-grid heavy admin UI; i18n vi/en |
| Maps / GIS | **MapLibre GL** or **Leaflet** + PostGIS | Clusters, petrol stations, markets |
| Charts | ECharts / Recharts | Leadership dashboards |
| Identity | **Keycloak** (OIDC/OAuth2) | Centralized auth; future provincial SSO; realm roles + groups |
| Database | **PostgreSQL 16 + PostGIS**; streaming **read replica** | Partitioning, FTS, JSONB |
| Cache | **Redis** | Sessions/reference data/rate-limit |
| Messaging | **RabbitMQ** + **MassTransit** | Domain events, outbox dispatch, saga |
| Object storage | **MinIO** (S3 API) | File/resource module; report attachments |
| Background jobs | **Quartz.NET / Hangfire** in the Worker host | Period scheduling, retries, cleanup |
| Observability | **Serilog → Seq/Loki**, **OpenTelemetry**, **Prometheus + Grafana** | Logs, traces, metrics, connection-status feed |
| Reverse proxy | **Nginx** (or YARP) | TLS termination, static hosting, routing |
| Containerization | **Docker + Docker Compose** | Per §4 |
| CI/CD | **GitHub Actions / GitLab CI** | Build, test, scan, image push, deploy |
| Secrets | Docker secrets / **HashiCorp Vault** (optional) | No secrets in images |

## 4. Deployment topology (Docker Compose, 1–2 servers)

**Two-server layout (recommended):**

- **App server** — `nginx`, `api` (1–3 replicas), `worker`, `keycloak`, `redis`, `rabbitmq`, `minio`,
  observability stack.
- **Data server** — `postgres-primary`, `postgres-replica`, backup agent.

If only one server is available, co-locate everything and keep the replica on the same host (still
useful for read isolation + faster restore). Compose profiles separate `app`, `data`, and `observability`
so they can move to the second host later.

Each module is **independently deployable in principle**, but in this topology the `api` container
hosts all modules. When load or team structure demands it, extract a module by giving it its own
Compose service + dedicated schema and switching its inbound contract from in-process to HTTP.

## 5. Cross-cutting concerns

- **Authentication:** OIDC via Keycloak; the API validates JWTs; supports centralized/shared accounts
  with provincial systems (requirement 4.2). Service-to-service uses client-credentials.
- **Authorization (the two-dimensional model):**
  - *Function-scope* → ASP.NET Core **policy-based authorization**; permissions map to Keycloak roles.
  - *Data-scope* → a **`IDataScopeFilter`** applied in the Application layer/EF query filters that
    restricts rows to the org-units a principal may access (resolved from the org tree). Implemented
    as a MediatR pipeline behavior + EF global query filter.
- **Validation:** FluentValidation behavior runs before every command/query handler.
- **Auditing:** an audit behavior records who/what/when for every state-changing command; deletes are
  always logged (requirement G1). Stored in an append-only, monthly-partitioned table.
- **Transactions & reliability:** Unit-of-Work per command; the **Outbox pattern** persists domain
  events in the same transaction and the Worker dispatches them to RabbitMQ (no lost events).
- **Error handling:** **Result/Problem-Details** pattern; consistent RFC 7807 API errors; no leaking
  internals.
- **Caching:** reference/catalog data cached in Redis with event-driven invalidation.
- **Observability:** structured logs (Serilog), distributed traces (OpenTelemetry), metrics
  (Prometheus). The **connection-status endpoint** (requirement F2) aggregates health checks of DB,
  Redis, RabbitMQ, MinIO, Keycloak, and registered external services.
- **Security hardening (Level 3):** TLS everywhere, secrets management, least-privilege DB roles,
  WAF/rate-limit at Nginx, input validation, output encoding, anti-CSRF for cookie flows, audit,
  encrypted backups, periodic vulnerability scans in CI (`dotnet`/npm audit, Trivy image scan).

## 6. Design patterns catalogue (and where each is used)

| Pattern | Applied to |
|---------|-----------|
| **DDD tactical** (Aggregate, Entity, Value Object, Domain Event, Domain Service, Repository) | All bounded contexts (Doc 03) |
| **Clean/Onion Architecture** | Every module's layer structure (§2) |
| **CQRS** | Command vs. query separation via MediatR; dedicated read models/materialized views for analytics |
| **Mediator + Pipeline (Decorator)** | Validation, logging, transaction, authorization, audit behaviors |
| **Specification** | The pervasive list/search/filter/paginate logic in *every* use case — composable, testable query specs |
| **Outbox / Transactional Messaging** | Reliable publication of domain events |
| **Saga / Process Manager** | Long-running report-submission & approval workflow (Doc 03 §5) |
| **State** | Report lifecycle (Draft → Submitted → … → Approved/Rejected) |
| **Strategy** | Import format handlers (Excel/XML/manual); indicator-type-specific computation |
| **Factory** | Constructing aggregates from imports/DTOs with invariant checks |
| **Anti-Corruption Layer (Adapter)** | Integration context wrapping LGSP/NDXP external models |
| **Repository + Unit of Work** | Persistence boundary per aggregate |
| **Policy / Strategy (authorization)** | Function-scope policies + data-scope filters |
| **Result / Notification** | Error propagation without exceptions for expected failures |
| **Options** | Strongly-typed configuration |
| **Publish/Subscribe** | In-module and cross-module domain events |

These patterns are not decoration — each maps to a concrete requirement (e.g. Specification ↔ the
repeated search/paginate requirement; Saga/State ↔ the approval workflow; ACL ↔ gov integration).
