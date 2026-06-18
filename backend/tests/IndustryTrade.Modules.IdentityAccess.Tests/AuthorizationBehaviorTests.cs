using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Behaviors;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using MediatR;
using Xunit;

namespace IndustryTrade.Modules.IdentityAccess.Tests;

public class AuthorizationBehaviorTests
{
    private sealed record FakeRequest(string RequiredPermission) : IPermissionAuthorized;

    private static readonly RequestHandlerDelegate<string> Next = () => Task.FromResult("ok");

    [Fact]
    public async Task Throws_when_unauthenticated()
    {
        var behavior = new AuthorizationBehavior<FakeRequest, string>(new FakeUser(authenticated: false));
        var act = () => behavior.Handle(new FakeRequest("identity.orgunits.read"), Next, default);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Throws_when_missing_permission()
    {
        var behavior = new AuthorizationBehavior<FakeRequest, string>(new FakeUser(authenticated: true));
        var act = () => behavior.Handle(new FakeRequest("identity.orgunits.manage"), Next, default);
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Passes_when_permission_present()
    {
        var behavior = new AuthorizationBehavior<FakeRequest, string>(
            new FakeUser(authenticated: true, perms: "identity.orgunits.manage"));
        var result = await behavior.Handle(new FakeRequest("identity.orgunits.manage"), Next, default);
        result.Should().Be("ok");
    }

    [Fact]
    public async Task SuperAdmin_bypasses_permission_check()
    {
        var behavior = new AuthorizationBehavior<FakeRequest, string>(
            new FakeUser(authenticated: true, superAdmin: true));
        var result = await behavior.Handle(new FakeRequest("anything"), Next, default);
        result.Should().Be("ok");
    }

    private sealed class FakeUser(bool authenticated, bool superAdmin = false, params string[] perms) : ICurrentUser
    {
        public bool IsAuthenticated => authenticated;
        public string? UserId => "test";
        public string? UserName => "test";
        public bool IsSuperAdmin => superAdmin;
        public IReadOnlySet<string> Permissions => perms.ToHashSet();
        public IReadOnlyCollection<string> DataScopePaths => [];
        public IReadOnlyCollection<Guid> DataScopeUnitIds => [];
        public bool HasPermission(string permission) => superAdmin || perms.Contains(permission);
    }
}
