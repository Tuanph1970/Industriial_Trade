using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Application.Violations;

namespace IndustryTrade.Modules.SectorData.Application;

// Hard-delete commands for the sector list pages. All flow through the audit behavior, so deletes
// are logged (design G1). Data-scope on delete is intentionally omitted here for brevity — add the
// same DataScopeUnitIds check used in the queries if per-row delete authorization is required.

public sealed record DeleteClusterCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ClustersManage;
}
public sealed class DeleteClusterHandler(IClusterRepository repo) : ICommandHandler<DeleteClusterCommand>
{
    public async Task<Result> Handle(DeleteClusterCommand c, CancellationToken ct) =>
        await repo.DeleteAsync(c.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Cluster"));
}

public sealed record DeletePetrolStationCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.PetrolManage;
}
public sealed class DeletePetrolStationHandler(IPetrolStationRepository repo) : ICommandHandler<DeletePetrolStationCommand>
{
    public async Task<Result> Handle(DeletePetrolStationCommand c, CancellationToken ct) =>
        await repo.DeleteAsync(c.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Petroleum station"));
}

public sealed record DeleteCommerceLocationCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.CommerceManage;
}
public sealed class DeleteCommerceLocationHandler(ICommerceLocationRepository repo) : ICommandHandler<DeleteCommerceLocationCommand>
{
    public async Task<Result> Handle(DeleteCommerceLocationCommand c, CancellationToken ct) =>
        await repo.DeleteAsync(c.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Commerce location"));
}

public sealed record DeleteEcommerceCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.EcommerceManage;
}
public sealed class DeleteEcommerceHandler(IEcommerceParticipantRepository repo) : ICommandHandler<DeleteEcommerceCommand>
{
    public async Task<Result> Handle(DeleteEcommerceCommand c, CancellationToken ct) =>
        await repo.DeleteAsync(c.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("E-commerce participant"));
}

public sealed record DeleteViolationCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ViolationsManage;
}
public sealed class DeleteViolationHandler(IViolationRepository repo) : ICommandHandler<DeleteViolationCommand>
{
    public async Task<Result> Handle(DeleteViolationCommand c, CancellationToken ct) =>
        await repo.DeleteAsync(c.Id, ct) ? Result.Success() : Result.Failure(Error.NotFound("Violation case"));
}
