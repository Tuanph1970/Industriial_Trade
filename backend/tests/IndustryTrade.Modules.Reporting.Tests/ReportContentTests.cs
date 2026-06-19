using FluentAssertions;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Xunit;

namespace IndustryTrade.Modules.Reporting.Tests;

public class ReportContentTests
{
    private static ReportLine Line(int order) => new()
    {
        IndicatorId = Guid.NewGuid(), IndicatorCode = $"CT{order}", Label = $"Chỉ tiêu {order}",
        RowOrder = order, Value = order * 10m, ValueText = null,
    };

    [Fact]
    public void SetContent_binds_template_and_lines_in_draft()
    {
        var s = ReportSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), "Báo cáo CN", "commune");
        var templateId = Guid.NewGuid();

        s.SetContent(templateId, [Line(2), Line(1)]);

        s.TemplateId.Should().Be(templateId);
        s.Lines.Should().HaveCount(2);
    }

    [Fact]
    public void SetContent_replaces_previous_lines()
    {
        var s = ReportSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), "Báo cáo CN", "commune");
        s.SetContent(Guid.NewGuid(), [Line(1), Line(2), Line(3)]);
        s.SetContent(Guid.NewGuid(), [Line(1)]);

        s.Lines.Should().ContainSingle();
    }

    [Fact]
    public void Cannot_set_content_after_submitted()
    {
        var s = ReportSubmission.Create(Guid.NewGuid(), Guid.NewGuid(), "Báo cáo CN", "commune");
        s.Submit("commune");

        var act = () => s.SetContent(Guid.NewGuid(), [Line(1)]);
        act.Should().Throw<BusinessRuleException>();
    }
}
