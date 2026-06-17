using IndustryTrade.BuildingBlocks.Domain;
using MediatR;

namespace IndustryTrade.BuildingBlocks.Application.Messaging;

// Thin CQRS abstractions over MediatR so handlers depend on our contracts, not the library directly.
// (Lets us swap the mediator implementation without touching application code.)

public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
