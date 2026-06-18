namespace IndustryTrade.Modules.Reporting.Domain.Submissions;

/// <summary>The report-submission lifecycle (docs/design/03 §5).</summary>
public enum ReportState
{
    Draft = 1,           // commune official is preparing
    Submitted = 2,       // sent to the Department
    UnderReview = 3,     // specialist is checking
    PendingApproval = 4, // forwarded to the division leader
    Approved = 5,        // leader approved
    Rejected = 6         // leader rejected (commune may reopen → Draft)
}

/// <summary>One recorded step in a submission's workflow history.</summary>
public sealed class ReportTransition
{
    public ReportState FromState { get; init; }
    public ReportState ToState { get; init; }
    public string Action { get; init; } = default!;
    public string? ActorName { get; init; }
    public DateTime AtUtc { get; init; }
    public string? Note { get; init; }
}

/// <summary>Raised on every state change; the saga/outbox turns these into notifications.</summary>
public sealed record ReportStateChanged(Guid SubmissionId, ReportState From, ReportState To, string Action)
    : IndustryTrade.BuildingBlocks.Domain.DomainEvent;
