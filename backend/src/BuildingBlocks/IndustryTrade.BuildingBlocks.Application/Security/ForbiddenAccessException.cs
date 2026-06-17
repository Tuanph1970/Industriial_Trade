namespace IndustryTrade.BuildingBlocks.Application.Security;

/// <summary>Thrown when the principal lacks the required permission. Mapped to HTTP 403 at the edge.</summary>
public sealed class ForbiddenAccessException(string message = "Access denied.") : Exception(message);
