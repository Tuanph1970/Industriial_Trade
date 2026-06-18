using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;

namespace IndustryTrade.Modules.SectorData.Application.PetroleumStations;

public sealed record CreatePetrolStationCommand(
    string Code, string Name, Guid OrgUnitId, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.PetrolManage;
}

public sealed class CreatePetrolStationValidator : AbstractValidator<CreatePetrolStationCommand>
{
    public CreatePetrolStationValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class CreatePetrolStationHandler(IPetrolStationRepository repository)
    : ICommandHandler<CreatePetrolStationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreatePetrolStationCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Station code '{command.Code}' already exists."));

        var station = PetroleumStation.Create(command.Code, command.Name, command.OrgUnitId,
            command.LicenseNo, command.Address, command.Longitude, command.Latitude, command.Status);
        await repository.AddAsync(station, ct);
        await repository.SaveChangesAsync(ct);
        return station.Id;
    }
}

public sealed record GetPetrolStationsQuery(PageRequest Page)
    : IQuery<PagedResult<PetrolStationDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.PetrolRead;
}

public sealed class GetPetrolStationsHandler(IPetrolStationRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetPetrolStationsQuery, PagedResult<PetrolStationDto>>
{
    public async Task<Result<PagedResult<PetrolStationDto>>> Handle(GetPetrolStationsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();
        var items = await repository.ListAsync(new PetrolStationSearchSpec(page, scope), ct);
        var total = await repository.CountAsync(new PetrolStationSearchSpec(page, scope, forCount: true), ct);
        return new PagedResult<PetrolStationDto>(items.Select(PetrolStationDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
