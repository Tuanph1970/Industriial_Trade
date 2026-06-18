using FluentAssertions;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Xunit;

namespace IndustryTrade.Modules.Reporting.Tests;

public class ReportSubmissionStateMachineTests
{
    private static ReportSubmission NewDraft() =>
        ReportSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), "Báo cáo tháng 6", "chuyenvien");

    [Fact]
    public void Happy_path_runs_draft_to_approved_and_records_history()
    {
        var s = NewDraft();
        s.State.Should().Be(ReportState.Draft);

        s.Submit("commune");
        s.AcceptForReview("specialist");
        s.ForwardForApproval("specialist");
        s.Approve("leader");

        s.State.Should().Be(ReportState.Approved);
        // Created + 4 transitions
        s.History.Should().HaveCount(5);
        s.History.Last().Action.Should().Be("Approve");
    }

    [Fact]
    public void Approving_a_draft_violates_the_state_machine()
    {
        var s = NewDraft();
        var act = () => s.Approve("leader");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Specialist_can_return_a_submitted_report_to_draft()
    {
        var s = NewDraft();
        s.Submit("commune");
        s.ReturnToDraft("specialist", "Thiếu số liệu");
        s.State.Should().Be(ReportState.Draft);
        s.History.Last().Note.Should().Be("Thiếu số liệu");
    }

    [Fact]
    public void Rejected_report_can_be_reopened_then_resubmitted()
    {
        var s = NewDraft();
        s.Submit("commune");
        s.AcceptForReview("specialist");
        s.ForwardForApproval("specialist");
        s.Reject("leader", "Không đạt");
        s.State.Should().Be(ReportState.Rejected);

        s.Reopen("commune");
        s.State.Should().Be(ReportState.Draft);
        s.Submit("commune");
        s.State.Should().Be(ReportState.Submitted);
    }

    [Fact]
    public void Cannot_submit_twice()
    {
        var s = NewDraft();
        s.Submit("commune");
        var act = () => s.Submit("commune");
        act.Should().Throw<BusinessRuleException>();
    }
}
