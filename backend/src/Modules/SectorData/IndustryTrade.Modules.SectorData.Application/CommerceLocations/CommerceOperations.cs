using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;

namespace IndustryTrade.Modules.SectorData.Application.CommerceLocations;

public sealed record CreateCommerceLocationCommand(
    string Code, string Name, CommerceLocationType Type, Guid OrgUnitId,
    string? Address, double? Latitude, double? Longitude)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.CommerceManage;
}

public sealed class CreateCommerceLocationValidator : AbstractValidator<CreateCommerceLocationCommand>
{
    public CreateCommerceLocationValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}

public sealed class CreateCommerceLocationHandler(ICommerceLocationRepository repository)
    : ICommandHandler<CreateCommerceLocationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommerceLocationCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Location code '{command.Code}' already exists."));

        var location = CommerceLocation.Create(command.Code, command.Name, command.Type,
            command.OrgUnitId, command.Address, command.Longitude, command.Latitude);
        await repository.AddAsync(location, ct);
        await repository.SaveChangesAsync(ct);
        return location.Id;
    }
}

public sealed record GetCommerceLocationsQuery(PageRequest Page, CommerceLocationType? Type)
    : IQuery<PagedResult<CommerceLocationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.CommerceRead;
}

public sealed class GetCommerceLocationsHandler(ICommerceLocationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetCommerceLocationsQuery, PagedResult<CommerceLocationDto>>
{
    public async Task<Result<PagedResult<CommerceLocationDto>>> Handle(GetCommerceLocationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();
        var items = await repository.ListAsync(new CommerceLocationSearchSpec(page, scope, query.Type), ct);
        var total = await repository.CountAsync(new CommerceLocationSearchSpec(page, scope, query.Type, forCount: true), ct);
        return new PagedResult<CommerceLocationDto>(items.Select(CommerceLocationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
