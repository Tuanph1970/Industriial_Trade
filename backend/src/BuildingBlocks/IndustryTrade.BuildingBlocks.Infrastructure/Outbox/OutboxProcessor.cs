using System.Text.Json;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IndustryTrade.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Transactional outbox DELIVERY side: polls one context's <see cref="OutboxMessage"/> table, wraps
/// each stored domain event in a <see cref="DomainEventNotification{TEvent}"/> and publishes it
/// in-process via MediatR (so <see cref="INotificationHandler{T}"/> implementations react), then
/// marks the row processed. At-least-once delivery (docs/design/02 §5).
/// In production this is where you would instead publish to RabbitMQ for cross-service delivery.
/// </summary>
public sealed class OutboxProcessor<TContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor<TContext>> logger) : BackgroundService
    where TContext : DbContext
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started for {Context}.", typeof(TContext).Name);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processing failed for {Context}.", typeof(TContext).Name);
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    message.Error = $"Unknown type '{message.Type}'.";
                }
                else if (JsonSerializer.Deserialize(message.Content, eventType) is IDomainEvent domainEvent)
                {
                    var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
                    var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
                    await publisher.Publish(notification, ct);
                }
            }
            catch (Exception ex)
            {
                // Record the error and mark processed to avoid a poison-message loop in this skeleton.
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to dispatch outbox message {Id}.", message.Id);
            }
            message.ProcessedOnUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}

public static class OutboxProcessorExtensions
{
    /// <summary>Runs an outbox processor for the given DbContext as a hosted background service.</summary>
    public static IServiceCollection AddOutboxProcessor<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddHostedService<OutboxProcessor<TContext>>();
        return services;
    }
}
