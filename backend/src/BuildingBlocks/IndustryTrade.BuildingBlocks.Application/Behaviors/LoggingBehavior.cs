using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IndustryTrade.BuildingBlocks.Application.Behaviors;

/// <summary>Structured request/response logging + timing for every command and query.</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Handling {RequestName}", name);
        try
        {
            var response = await next();
            logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs} ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}

// TODO (Phase 1): add TransactionBehavior, AuthorizationBehavior (function-scope + data-scope),
// and AuditBehavior here — see docs/design/02 §5. They plug into the same pipeline.
