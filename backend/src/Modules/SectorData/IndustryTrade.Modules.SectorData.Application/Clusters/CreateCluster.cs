using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.Clusters;

namespace IndustryTrade.Modules.SectorData.Application.Clusters;

public sealed record CreateClusterCommand(
    string Code, string Name, Guid OrgUnitId, decimal? AreaHa,
    double? Latitude, double? Longitude, ClusterStatus Status)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ClustersManage;
}

public sealed class CreateClusterValidator : AbstractValidator<CreateClusterCommand>
{
    public CreateClusterValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class CreateClusterHandler(IClusterRepository repository)
    : ICommandHandler<CreateClusterCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateClusterCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Cluster code '{command.Code}' already exists."));

        var cluster = IndustrialCluster.Create(command.Code, command.Name, command.OrgUnitId,
            command.AreaHa, command.Longitude, command.Latitude, command.Status);
        await repository.AddAsync(cluster, ct);
        await repository.SaveChangesAsync(ct);
        return cluster.Id;
    }
}
