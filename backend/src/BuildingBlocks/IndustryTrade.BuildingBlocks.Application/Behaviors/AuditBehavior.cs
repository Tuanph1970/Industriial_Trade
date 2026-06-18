using System.Text.Json;
using IndustryTrade.BuildingBlocks.Application.Auditing;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IndustryTrade.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Records every command (anything implementing <see cref="IBaseCommand"/>) to the audit log:
/// who, what (command type + JSON payload), and the outcome. Best-effort and append-only — an audit
/// write failure never breaks the command. Register innermost so it observes the real result.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditSink auditSink,
    ICurrentUser currentUser,
    ILogger<AuditBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IBaseCommand)
            return await next();

        try
        {
            var response = await next();
            var success = response is not Result { IsFailure: true };
            var error = response is Result { IsFailure: true } r ? r.Error.Message : null;
            await TryWriteAsync(request, success, error, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            await TryWriteAsync(request, success: false, error: ex.GetType().Name, cancellationToken);
            throw;
        }
    }

    private async Task TryWriteAsync(TRequest request, bool success, string? error, CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Serialize(request, request.GetType());
            await auditSink.WriteAsync(
                new AuditEntry(currentUser.UserName, typeof(TRequest).Name, payload, success, error), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write audit entry for {Action}.", typeof(TRequest).Name);
        }
    }
}
