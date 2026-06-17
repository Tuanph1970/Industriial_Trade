namespace IndustryTrade.BuildingBlocks.Application.Messaging;

/// <summary>
/// Marks a command/query that requires a function-scope permission. The AuthorizationBehavior
/// enforces it in the pipeline, so the rule holds no matter which entry point dispatched the request.
/// </summary>
public interface IPermissionAuthorized
{
    string RequiredPermission { get; }
}
