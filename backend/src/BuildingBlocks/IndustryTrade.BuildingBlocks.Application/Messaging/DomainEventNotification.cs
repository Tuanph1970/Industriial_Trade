using IndustryTrade.BuildingBlocks.Domain;
using MediatR;

namespace IndustryTrade.BuildingBlocks.Application.Messaging;

/// <summary>
/// Wraps a domain event as a MediatR notification so the outbox processor can publish it in-process
/// to <see cref="INotificationHandler{T}"/> implementations — without the Domain layer depending on
/// MediatR. Handlers implement <c>INotificationHandler&lt;DomainEventNotification&lt;TEvent&gt;&gt;</c>.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
