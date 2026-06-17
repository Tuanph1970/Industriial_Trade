namespace IndustryTrade.Worker;

/// <summary>
/// Placeholder for the transactional-outbox dispatcher: polls each module's outbox table and
/// publishes pending domain events to RabbitMQ, then marks them processed (docs/design/02 §5).
/// The real implementation lands in Phase 1 alongside the first cross-module event.
/// </summary>
public sealed class OutboxDispatcher(ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox dispatcher started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: scan {schema}.outbox_message WHERE processed_on_utc IS NULL → publish → mark processed.
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
