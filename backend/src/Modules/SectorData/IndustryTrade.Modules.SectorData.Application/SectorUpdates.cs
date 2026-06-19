using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Application.Violations;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using IndustryTrade.Modules.SectorData.Domain.Violations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;

namespace IndustryTrade.Modules.SectorData.Application;

// Update commands for the sector list pages — edit the mutable fields of an existing record.
// The natural key (code / tax code / case no) is immutable, mirroring the Catalog Indicator edit.
// All flow through validation + the audit behavior (design G1).

// ── Cluster ────────────────────────────────────────────────────────────────
public sealed record UpdateClusterCommand(
    Guid Id, string Name, decimal? AreaHa, double? Latitude, double? Longitude, ClusterStatus Status)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ClustersManage;
}

public sealed class UpdateClusterValidator : AbstractValidator<UpdateClusterCommand>
{
    public UpdateClusterValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateClusterHandler(IClusterRepository repo) : ICommandHandler<UpdateClusterCommand>
{
    public async Task<Result> Handle(UpdateClusterCommand c, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(c.Id, ct);
        if (entity is null) return Result.Failure(Error.NotFound("Cluster"));

        entity.Update(c.Name, c.AreaHa, c.Longitude, c.Latitude, c.Status);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Petroleum station ────────────────────────────────────────────────────────
public sealed record UpdatePetrolStationCommand(
    Guid Id, string Name, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.PetrolManage;
}

public sealed class UpdatePetrolStationValidator : AbstractValidator<UpdatePetrolStationCommand>
{
    public UpdatePetrolStationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdatePetrolStationHandler(IPetrolStationRepository repo) : ICommandHandler<UpdatePetrolStationCommand>
{
    public async Task<Result> Handle(UpdatePetrolStationCommand c, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(c.Id, ct);
        if (entity is null) return Result.Failure(Error.NotFound("Petroleum station"));

        entity.Update(c.Name, c.LicenseNo, c.Address, c.Longitude, c.Latitude, c.Status);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Commerce location ────────────────────────────────────────────────────────
public sealed record UpdateCommerceLocationCommand(
    Guid Id, string Name, CommerceLocationType Type, string? Address, double? Latitude, double? Longitude)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.CommerceManage;
}

public sealed class UpdateCommerceLocationValidator : AbstractValidator<UpdateCommerceLocationCommand>
{
    public UpdateCommerceLocationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}

public sealed class UpdateCommerceLocationHandler(ICommerceLocationRepository repo) : ICommandHandler<UpdateCommerceLocationCommand>
{
    public async Task<Result> Handle(UpdateCommerceLocationCommand c, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(c.Id, ct);
        if (entity is null) return Result.Failure(Error.NotFound("Commerce location"));

        entity.Update(c.Name, c.Type, c.Address, c.Longitude, c.Latitude);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── E-commerce participant ────────────────────────────────────────────────────
public sealed record UpdateEcommerceCommand(
    Guid Id, string BusinessName, string[] Platforms, string? MainGoods)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.EcommerceManage;
}

public sealed class UpdateEcommerceValidator : AbstractValidator<UpdateEcommerceCommand>
{
    public UpdateEcommerceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(250);
    }
}

public sealed class UpdateEcommerceHandler(IEcommerceParticipantRepository repo) : ICommandHandler<UpdateEcommerceCommand>
{
    public async Task<Result> Handle(UpdateEcommerceCommand c, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(c.Id, ct);
        if (entity is null) return Result.Failure(Error.NotFound("E-commerce participant"));

        entity.Update(c.BusinessName, c.Platforms ?? [], c.MainGoods);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Market violation case ──────────────────────────────────────────────────────
public sealed record UpdateViolationCommand(
    Guid Id, ViolationGroup Group, string BusinessName, DateOnly InspectedOn, string ViolationContent,
    string? SanctionContent, decimal? FineAmount, ViolationStatus Status)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ViolationsManage;
}

public sealed class UpdateViolationValidator : AbstractValidator<UpdateViolationCommand>
{
    public UpdateViolationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Group).IsInEnum();
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ViolationContent).NotEmpty();
        RuleFor(x => x.FineAmount).GreaterThanOrEqualTo(0).When(x => x.FineAmount.HasValue);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateViolationHandler(IViolationRepository repo) : ICommandHandler<UpdateViolationCommand>
{
    public async Task<Result> Handle(UpdateViolationCommand c, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(c.Id, ct);
        if (entity is null) return Result.Failure(Error.NotFound("Violation case"));

        entity.Update(c.Group, c.BusinessName, c.InspectedOn, c.ViolationContent,
            c.SanctionContent, c.FineAmount, c.Status);
        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
