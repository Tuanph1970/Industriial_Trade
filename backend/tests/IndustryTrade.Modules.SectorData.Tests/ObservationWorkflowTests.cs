using FluentAssertions;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using Xunit;

namespace IndustryTrade.Modules.SectorData.Tests;

public class ObservationWorkflowTests
{
    private static IndicatorObservation NewDraft() =>
        IndicatorObservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2026, 6, 100m, null, null);

    [Fact]
    public void New_observation_starts_in_draft()
    {
        NewDraft().Status.Should().Be(ObservationStatus.Draft);
    }

    [Fact]
    public void Submit_then_approve_walks_the_happy_path()
    {
        var o = NewDraft();
        o.Submit();
        o.Status.Should().Be(ObservationStatus.Submitted);
        o.Approve();
        o.Status.Should().Be(ObservationStatus.Approved);
    }

    [Fact]
    public void Return_sends_a_submitted_observation_back_to_draft()
    {
        var o = NewDraft();
        o.Submit();
        o.ReturnToDraft();
        o.Status.Should().Be(ObservationStatus.Draft);
    }

    [Fact]
    public void Cannot_approve_a_draft()
    {
        var act = () => NewDraft().Approve();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cannot_submit_twice()
    {
        var o = NewDraft();
        o.Submit();
        var act = () => o.Submit();
        act.Should().Throw<InvalidOperationException>();
    }
}
