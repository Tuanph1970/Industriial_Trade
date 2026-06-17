using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;
using Xunit;

namespace IndustryTrade.Modules.IdentityAccess.Tests;

public class CreateOrgUnitHandlerTests
{
    [Fact]
    public async Task Creates_unit_and_returns_id()
    {
        var repo = new InMemoryOrgUnitRepository();
        var handler = new CreateOrgUnitHandler(repo);

        var result = await handler.Handle(
            new CreateOrgUnitCommand("SCT", "Sở Công Thương", OrgUnitType.Department, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Items.Should().ContainSingle(u => u.Id == result.Value);
    }

    [Fact]
    public async Task Fails_when_parent_does_not_exist()
    {
        var handler = new CreateOrgUnitHandler(new InMemoryOrgUnitRepository());

        var result = await handler.Handle(
            new CreateOrgUnitCommand("X01", "Xã 01", OrgUnitType.Commune, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("not_found");
    }

    private sealed class InMemoryOrgUnitRepository : IOrgUnitRepository
    {
        public List<OrgUnit> Items { get; } = new();

        public Task<OrgUnit?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<OrgUnit>> ListAsync(Specification<OrgUnit> spec, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<OrgUnit>>(Items);

        public Task<int> CountAsync(Specification<OrgUnit> spec, CancellationToken ct) =>
            Task.FromResult(Items.Count);

        public Task AddAsync(OrgUnit unit, CancellationToken ct)
        {
            Items.Add(unit);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => Task.FromResult(0);
    }
}
