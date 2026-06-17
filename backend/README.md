# Backend — IndustryTrade (.NET 10)

DDD modular monolith. Bounded contexts are separate modules composed by the `IndustryTrade.Api`
host; each is extractable into its own service later (see `../docs/design/02-solution-architecture.md`).

## Prerequisites

- **.NET 10 SDK.** On this Ubuntu 20.04 box it is installed user-locally — the system `dotnet` is 9.x
  (EOL). Prefix commands with the local SDK, or put it on PATH:
  ```bash
  export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="$HOME/.dotnet"
  ```
- PostgreSQL+PostGIS, Redis, RabbitMQ, etc. — via `../deploy/docker-compose.yml`.

## Common commands

```bash
dotnet build                      # build the whole solution (IndustryTrade.slnx)
dotnet test                       # run all tests
dotnet test tests/IndustryTrade.Modules.IdentityAccess.Tests   # one test project
dotnet test --filter "FullyQualifiedName~OrgUnitTests"          # one test class/method
dotnet run --project src/Hosts/IndustryTrade.Api                # run API (Swagger at /swagger)
dotnet run --project src/Hosts/IndustryTrade.Worker             # run background worker
```

The API auto-applies EF migrations on startup **in Development**. In production, run migrations as an
explicit deploy step.

## EF Core migrations (per module)

`dotnet-ef` is pinned as a local tool (`dotnet-tools.json`). Each module owns its `DbContext` and
schema. Point `-p` at the module's Infrastructure project and `-s` at the API host:

```bash
dotnet dotnet-ef migrations add <Name> \
  -p src/Modules/IdentityAccess/IndustryTrade.Modules.IdentityAccess.Infrastructure \
  -s src/Hosts/IndustryTrade.Api \
  -o Persistence/Migrations
```

## Layout

```
src/
  BuildingBlocks/        Domain primitives, CQRS+pipeline, EF/specification, web/module abstraction
  Modules/
    IdentityAccess/      Reference module: Domain → Application → Infrastructure → Api (implements IModule)
  Hosts/
    IndustryTrade.Api/   ASP.NET Core host; composes modules, pipeline, Swagger, migrations
    IndustryTrade.Worker/Background host; MassTransit + outbox dispatcher
tests/                   xUnit + FluentAssertions
```

## Adding a new bounded context

Copy the IdentityAccess four-project shape, then:
1. `Domain` → aggregates/value-objects/events (reference `BuildingBlocks.Domain`).
2. `Application` → commands/queries (`ICommand`/`IQuery`), validators, repository ports, specs.
3. `Infrastructure` → `DbContext` on its **own schema**, EF configs, repositories, `Add<Module>Infrastructure`.
4. `Api` → a class implementing `IModule` (RegisterServices + MapEndpoints + ApplyMigrationsAsync) and endpoints.
5. Register the module in `IndustryTrade.Api/Program.cs` (`IModule[] modules = [...]`) and add the
   project reference in `IndustryTrade.Api.csproj`.
