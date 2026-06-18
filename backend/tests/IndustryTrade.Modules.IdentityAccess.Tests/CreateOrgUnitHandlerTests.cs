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

    [Fact]
    public async Task Update_changes_name_and_active_flag()
    {
        var repo = new InMemoryOrgUnitRepository();
        var unit = OrgUnit.Create("SCT", "Old name", OrgUnitType.Department, null);
        repo.Items.Add(unit);

        var result = await new UpdateOrgUnitHandler(repo).Handle(
            new UpdateOrgUnitCommand(unit.Id, "New name", false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        unit.Name.Should().Be("New name");
        unit.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_removes_a_leaf_unit()
    {
        var repo = new InMemoryOrgUnitRepository();
        var unit = OrgUnit.Create("X1", "Leaf", OrgUnitType.Commune, null);
        repo.Items.Add(unit);

        var result = await new DeleteOrgUnitHandler(repo).Handle(
            new DeleteOrgUnitCommand(unit.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        repo.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_is_blocked_when_the_unit_has_children()
    {
        var repo = new InMemoryOrgUnitRepository();
        var parent = OrgUnit.Create("P", "Parent", OrgUnitType.Department, null);
        var child = OrgUnit.Create("C", "Child", OrgUnitType.Commune, parent);
        repo.Items.AddRange([parent, child]);

        var result = await new DeleteOrgUnitHandler(repo).Handle(
            new DeleteOrgUnitCommand(parent.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("conflict");
        repo.Items.Should().Contain(parent);
    }

    private sealed class InMemoryOrgUnitRepository : IOrgUnitRepository
    {
        public List<OrgUnit> Items { get; } = new();

        public Task<OrgUnit?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<bool> HasChildrenAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Items.Any(x => x.ParentId == id));

        public void Remove(OrgUnit unit) => Items.Remove(unit);

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
