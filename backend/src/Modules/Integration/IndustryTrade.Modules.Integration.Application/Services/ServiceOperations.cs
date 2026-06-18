using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Integration.Domain.Services;

namespace IndustryTrade.Modules.Integration.Application.Services;

public static class IntegrationPermissions
{
    public const string Read = "integration.read";
    public const string Manage = "integration.manage";
}

public sealed record ServiceDto(
    Guid Id, string Code, string Name, ServiceDirection Direction, string? EndpointUrl,
    string? Description, ServiceStatus Status)
{
    public static ServiceDto FromEntity(DataSharingService s) =>
        new(s.Id, s.Code, s.Name, s.Direction, s.EndpointUrl, s.Description, s.Status);
}

public interface IDataSharingServiceRepository
{
    Task<DataSharingService?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<DataSharingService>> ListAsync(Specification<DataSharingService> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<DataSharingService> spec, CancellationToken ct);
    Task<IReadOnlyList<DataSharingService>> GetPublishedAsync(CancellationToken ct);
    Task AddAsync(DataSharingService service, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class ServiceSearchSpec : Specification<DataSharingService>
{
    public ServiceSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(s => s.Code.ToLower().Contains(kw) || s.Name.ToLower().Contains(kw));
        }
        if (!forCount)
        {
            ApplyOrderBy(s => s.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record CreateServiceCommand(
    string Code, string Name, ServiceDirection Direction, string? EndpointUrl, string? Description)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => IntegrationPermissions.Manage;
}

public sealed class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Direction).IsInEnum();
    }
}

public sealed class CreateServiceHandler(IDataSharingServiceRepository repository)
    : ICommandHandler<CreateServiceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Service code '{command.Code}' already exists."));

        var service = DataSharingService.Create(
            command.Code, command.Name, command.Direction, command.EndpointUrl, command.Description);
        await repository.AddAsync(service, ct);
        await repository.SaveChangesAsync(ct);
        return service.Id;
    }
}

public enum ServiceLifecycleAction { Publish, Revoke }

public sealed record ChangeServiceStatusCommand(Guid Id, ServiceLifecycleAction Action)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => IntegrationPermissions.Manage;
}

public sealed class ChangeServiceStatusHandler(IDataSharingServiceRepository repository)
    : ICommandHandler<ChangeServiceStatusCommand>
{
    public async Task<Result> Handle(ChangeServiceStatusCommand command, CancellationToken ct)
    {
        var service = await repository.GetByIdAsync(command.Id, ct);
        if (service is null)
            return Result.Failure(Error.NotFound("Data-sharing service"));

        if (command.Action == ServiceLifecycleAction.Publish) service.Publish();
        else service.Revoke();

        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed record GetServicesQuery(PageRequest Page) : IQuery<PagedResult<ServiceDto>>, IPermissionAuthorized
{
    public string RequiredPermission => IntegrationPermissions.Read;
}

public sealed class GetServicesHandler(IDataSharingServiceRepository repository)
    : IQueryHandler<GetServicesQuery, PagedResult<ServiceDto>>
{
    public async Task<Result<PagedResult<ServiceDto>>> Handle(GetServicesQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new ServiceSearchSpec(page), ct);
        var total = await repository.CountAsync(new ServiceSearchSpec(page, forCount: true), ct);
        return new PagedResult<ServiceDto>(items.Select(ServiceDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
