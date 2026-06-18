namespace IndustryTrade.BuildingBlocks.Domain;

/// <summary>
/// Thrown when an aggregate invariant or a state-machine transition rule is violated
/// (e.g. approving a report that is not pending approval). Mapped to HTTP 409 at the edge.
/// </summary>
public sealed class BusinessRuleException(string message) : Exception(message);
