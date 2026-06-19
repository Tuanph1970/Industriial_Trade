using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Reporting.Domain.Submissions;

/// <summary>
/// A report a base unit submits against a campaign, driven through the approval workflow
/// (commune → specialist → division leader). The transition methods are the State machine: each
/// guards the current state, records history, and raises <see cref="ReportStateChanged"/>.
/// </summary>
public sealed class ReportSubmission : AggregateRoot<Guid>, IAuditable
{
    private readonly List<ReportTransition> _history = new();
    private readonly List<ReportLine> _lines = new();

    private ReportSubmission() { } // EF

    private ReportSubmission(Guid id, Guid campaignId, Guid orgUnitId, string title) : base(id)
    {
        CampaignId = campaignId;
        OrgUnitId = orgUnitId;
        Title = title;
        State = ReportState.Draft;
    }

    public Guid CampaignId { get; private set; }
    public Guid OrgUnitId { get; private set; }
    public string Title { get; private set; } = default!;
    public ReportState State { get; private set; }
    /// <summary>The Catalog report template the content was extracted from, if any.</summary>
    public Guid? TemplateId { get; private set; }
    public IReadOnlyCollection<ReportTransition> History => _history.AsReadOnly();
    public IReadOnlyCollection<ReportLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static ReportSubmission Create(Guid campaignId, Guid orgUnitId, string title, string? actor)
    {
        if (campaignId == Guid.Empty) throw new ArgumentException("Campaign is required.", nameof(campaignId));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));

        var submission = new ReportSubmission(Guid.NewGuid(), campaignId, orgUnitId, title.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = actor
        };
        submission.Record(ReportState.Draft, ReportState.Draft, "Created", actor, null);
        return submission;
    }

    // ── Workflow transitions ────────────────────────────────────────────────
    public void Submit(string? actor) =>
        Transition(ReportState.Submitted, "Submit", actor, null, ReportState.Draft);

    public void AcceptForReview(string? actor) =>
        Transition(ReportState.UnderReview, "AcceptForReview", actor, null, ReportState.Submitted);

    public void ReturnToDraft(string? actor, string? note) =>
        Transition(ReportState.Draft, "Return", actor, note, ReportState.Submitted, ReportState.UnderReview);

    public void ForwardForApproval(string? actor) =>
        Transition(ReportState.PendingApproval, "ForwardForApproval", actor, null, ReportState.UnderReview);

    public void Approve(string? actor) =>
        Transition(ReportState.Approved, "Approve", actor, null, ReportState.PendingApproval);

    public void Reject(string? actor, string? note) =>
        Transition(ReportState.Rejected, "Reject", actor, note, ReportState.PendingApproval);

    public void Reopen(string? actor) =>
        Transition(ReportState.Draft, "Reopen", actor, null, ReportState.Rejected);

    /// <summary>Replaces the report content (auto-extracted lines). Only while still editable (Draft).</summary>
    public void SetContent(Guid templateId, IEnumerable<ReportLine> lines)
    {
        if (State != ReportState.Draft)
            throw new BusinessRuleException("Report content can only be set while the report is in Draft.");

        TemplateId = templateId;
        _lines.Clear();
        _lines.AddRange(lines);
        ModifiedAtUtc = DateTime.UtcNow;
    }

    private void Transition(ReportState to, string action, string? actor, string? note, params ReportState[] allowedFrom)
    {
        if (!allowedFrom.Contains(State))
            throw new BusinessRuleException(
                $"Cannot '{action}' a report in state '{State}'. Allowed from: {string.Join(", ", allowedFrom)}.");

        var from = State;
        State = to;
        ModifiedAtUtc = DateTime.UtcNow;
        ModifiedBy = actor;
        Record(from, to, action, actor, note);
        Raise(new ReportStateChanged(Id, from, to, action, OrgUnitId));
    }

    private void Record(ReportState from, ReportState to, string action, string? actor, string? note) =>
        _history.Add(new ReportTransition
        {
            FromState = from, ToState = to, Action = action, ActorName = actor, AtUtc = DateTime.UtcNow, Note = note
        });
}
