using FluentAssertions;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Integration.Domain.Services;
using Xunit;

namespace IndustryTrade.Modules.Integration.Tests;

public class DataSharingServiceTests
{
    private static DataSharingService New() =>
        DataSharingService.Create("SVC01", "Chia sẻ danh mục", ServiceDirection.Provide, "https://lgsp/api", null);

    [Fact]
    public void Create_starts_registered()
    {
        New().Status.Should().Be(ServiceStatus.Registered);
    }

    [Fact]
    public void Publish_then_revoke_follows_lifecycle()
    {
        var s = New();
        s.Publish();
        s.Status.Should().Be(ServiceStatus.Published);
        s.Revoke();
        s.Status.Should().Be(ServiceStatus.Revoked);
    }

    [Fact]
    public void Revoking_a_registered_service_is_invalid()
    {
        var act = () => New().Revoke();
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Publishing_a_revoked_service_is_invalid()
    {
        var s = New();
        s.Publish();
        s.Revoke();
        var act = () => s.Publish();
        act.Should().Throw<BusinessRuleException>();
    }
}
