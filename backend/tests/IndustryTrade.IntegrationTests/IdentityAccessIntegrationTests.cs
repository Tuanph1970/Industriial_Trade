using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IndustryTrade.IntegrationTests;

[Trait("Category", "Integration")]
[Collection("postgres")]
public sealed class IdentityAccessIntegrationTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Saving_an_aggregate_writes_its_domain_events_to_the_outbox()
    {
        await using var db = fixture.CreateContext();

        db.OrgUnits.Add(OrgUnit.Create("OBX1", "Outbox test unit", OrgUnitType.Department, parent: null));
        await db.SaveChangesAsync();

        // The OutboxWriterInterceptor must have persisted an OrgUnitCreated event in the same save.
        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.Content.Contains("OBX1"))
            .ToListAsync();

        messages.Should().ContainSingle();
        messages[0].Type.Should().Contain("OrgUnitCreated");
        messages[0].ProcessedOnUtc.Should().BeNull();
    }

    [Fact]
    public async Task Data_scope_specification_returns_only_the_unit_subtree()
    {
        await using var db = fixture.CreateContext();

        var root = OrgUnit.Create("SCOPER", "Scope root", OrgUnitType.Department, parent: null);
        db.OrgUnits.Add(root);
        await db.SaveChangesAsync();
        db.OrgUnits.Add(OrgUnit.Create("CHILD", "Scope child", OrgUnitType.Commune, parent: root));
        db.OrgUnits.Add(OrgUnit.Create("OTHERX", "Out of scope", OrgUnitType.Department, parent: null));
        await db.SaveChangesAsync();

        // Data-scope = subtree of "SCOPER"; "OTHERX" must be excluded. Exercises the PredicateBuilder
        // OR-of-StartsWith translation against real PostgreSQL.
        var spec = new OrgUnitSearchSpec(new PageRequest(1, 100), dataScopePaths: ["SCOPER"]);
        var results = await SpecificationEvaluator.Apply(db.OrgUnits.AsQueryable(), spec).ToListAsync();

        var codes = results.Select(r => r.Code).ToList();
        codes.Should().Contain(["SCOPER", "CHILD"]);
        codes.Should().NotContain("OTHERX");
    }
}
