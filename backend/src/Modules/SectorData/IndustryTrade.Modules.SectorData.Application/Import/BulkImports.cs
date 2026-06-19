using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Application.Clusters;
using IndustryTrade.Modules.SectorData.Application.CommerceLocations;
using IndustryTrade.Modules.SectorData.Application.Ecommerce;
using IndustryTrade.Modules.SectorData.Application.Observations;
using IndustryTrade.Modules.SectorData.Application.PetroleumStations;
using IndustryTrade.Modules.SectorData.Application.Violations;
using IndustryTrade.Modules.SectorData.Domain.Clusters;
using IndustryTrade.Modules.SectorData.Domain.CommerceLocations;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;
using IndustryTrade.Modules.SectorData.Domain.Observations;
using IndustryTrade.Modules.SectorData.Domain.PetroleumStations;
using IndustryTrade.Modules.SectorData.Domain.Violations;

namespace IndustryTrade.Modules.SectorData.Application.Import;

// Bulk-create commands backing batch import. The client resolves codes→ids and sends already-typed
// items; each handler validates per-row (domain invariants + duplicate keys), collects row errors,
// and persists the valid rows in one SaveChanges. Errors never abort the whole batch (partial import).

internal static class BulkImportLimits
{
    public const int MaxRows = 5000;
}

// ── Observations ────────────────────────────────────────────────────────────
public sealed record ObservationImportItem(
    Guid IndicatorId, Guid OrgUnitId, int PeriodYear, int? PeriodMonth,
    decimal? Value, string? ValueText, string? Source);

public sealed record BulkCreateObservationsCommand(IReadOnlyList<ObservationImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ObservationsManage;
}

public sealed class BulkCreateObservationsValidator : AbstractValidator<BulkCreateObservationsCommand>
{
    public BulkCreateObservationsValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreateObservationsHandler(IObservationRepository repo)
    : ICommandHandler<BulkCreateObservationsCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreateObservationsCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                var entity = IndicatorObservation.Create(
                    it.IndicatorId, it.OrgUnitId, it.PeriodYear, it.PeriodMonth, it.Value, it.ValueText, it.Source);
                await repo.AddAsync(entity, ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}

// ── Industrial clusters ──────────────────────────────────────────────────────
public sealed record ClusterImportItem(
    string Code, string Name, Guid OrgUnitId, decimal? AreaHa,
    double? Latitude, double? Longitude, ClusterStatus Status);

public sealed record BulkCreateClustersCommand(IReadOnlyList<ClusterImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ClustersManage;
}

public sealed class BulkCreateClustersValidator : AbstractValidator<BulkCreateClustersCommand>
{
    public BulkCreateClustersValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreateClustersHandler(IClusterRepository repo)
    : ICommandHandler<BulkCreateClustersCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreateClustersCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                if (!seen.Add(it.Code) || await repo.ExistsByCodeAsync(it.Code, ct))
                { errors.Add(new BulkRowError(i, $"Duplicate code '{it.Code}'.")); continue; }

                await repo.AddAsync(IndustrialCluster.Create(
                    it.Code, it.Name, it.OrgUnitId, it.AreaHa, it.Longitude, it.Latitude, it.Status), ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}

// ── Petroleum stations ───────────────────────────────────────────────────────
public sealed record PetrolStationImportItem(
    string Code, string Name, Guid OrgUnitId, string? LicenseNo, string? Address,
    double? Latitude, double? Longitude, StationStatus Status);

public sealed record BulkCreatePetrolStationsCommand(IReadOnlyList<PetrolStationImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.PetrolManage;
}

public sealed class BulkCreatePetrolStationsValidator : AbstractValidator<BulkCreatePetrolStationsCommand>
{
    public BulkCreatePetrolStationsValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreatePetrolStationsHandler(IPetrolStationRepository repo)
    : ICommandHandler<BulkCreatePetrolStationsCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreatePetrolStationsCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                if (!seen.Add(it.Code) || await repo.ExistsByCodeAsync(it.Code, ct))
                { errors.Add(new BulkRowError(i, $"Duplicate code '{it.Code}'.")); continue; }

                await repo.AddAsync(PetroleumStation.Create(
                    it.Code, it.Name, it.OrgUnitId, it.LicenseNo, it.Address, it.Longitude, it.Latitude, it.Status), ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}

// ── Commerce locations ───────────────────────────────────────────────────────
public sealed record CommerceLocationImportItem(
    string Code, string Name, CommerceLocationType Type, Guid OrgUnitId,
    string? Address, double? Latitude, double? Longitude);

public sealed record BulkCreateCommerceLocationsCommand(IReadOnlyList<CommerceLocationImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.CommerceManage;
}

public sealed class BulkCreateCommerceLocationsValidator : AbstractValidator<BulkCreateCommerceLocationsCommand>
{
    public BulkCreateCommerceLocationsValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreateCommerceLocationsHandler(ICommerceLocationRepository repo)
    : ICommandHandler<BulkCreateCommerceLocationsCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreateCommerceLocationsCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                if (!seen.Add(it.Code) || await repo.ExistsByCodeAsync(it.Code, ct))
                { errors.Add(new BulkRowError(i, $"Duplicate code '{it.Code}'.")); continue; }

                await repo.AddAsync(CommerceLocation.Create(
                    it.Code, it.Name, it.Type, it.OrgUnitId, it.Address, it.Longitude, it.Latitude), ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}

// ── E-commerce participants ──────────────────────────────────────────────────
public sealed record EcommerceImportItem(
    string TaxCode, string BusinessName, Guid OrgUnitId, string[] Platforms, string? MainGoods);

public sealed record BulkCreateEcommerceCommand(IReadOnlyList<EcommerceImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.EcommerceManage;
}

public sealed class BulkCreateEcommerceValidator : AbstractValidator<BulkCreateEcommerceCommand>
{
    public BulkCreateEcommerceValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreateEcommerceHandler(IEcommerceParticipantRepository repo)
    : ICommandHandler<BulkCreateEcommerceCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreateEcommerceCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                if (!seen.Add(it.TaxCode) || await repo.ExistsByTaxCodeAsync(it.TaxCode, ct))
                { errors.Add(new BulkRowError(i, $"Duplicate tax code '{it.TaxCode}'.")); continue; }

                await repo.AddAsync(EcommerceParticipant.Create(
                    it.TaxCode, it.BusinessName, it.OrgUnitId, it.Platforms ?? [], it.MainGoods), ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}

// ── Market violation cases ───────────────────────────────────────────────────
public sealed record ViolationImportItem(
    string CaseNo, ViolationGroup Group, Guid OrgUnitId, string BusinessName,
    DateOnly InspectedOn, string ViolationContent);

public sealed record BulkCreateViolationsCommand(IReadOnlyList<ViolationImportItem> Items)
    : ICommand<BulkImportResult>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ViolationsManage;
}

public sealed class BulkCreateViolationsValidator : AbstractValidator<BulkCreateViolationsCommand>
{
    public BulkCreateViolationsValidator() =>
        RuleFor(x => x.Items).NotEmpty().Must(i => i.Count <= BulkImportLimits.MaxRows)
            .WithMessage($"At most {BulkImportLimits.MaxRows} rows per import.");
}

public sealed class BulkCreateViolationsHandler(IViolationRepository repo)
    : ICommandHandler<BulkCreateViolationsCommand, BulkImportResult>
{
    public async Task<Result<BulkImportResult>> Handle(BulkCreateViolationsCommand command, CancellationToken ct)
    {
        var errors = new List<BulkRowError>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        for (var i = 0; i < command.Items.Count; i++)
        {
            var it = command.Items[i];
            try
            {
                if (!seen.Add(it.CaseNo) || await repo.ExistsByCaseNoAsync(it.CaseNo, ct))
                { errors.Add(new BulkRowError(i, $"Duplicate case no '{it.CaseNo}'.")); continue; }

                await repo.AddAsync(MarketViolationCase.Create(
                    it.CaseNo, it.Group, it.OrgUnitId, it.BusinessName, it.InspectedOn, it.ViolationContent), ct);
                created++;
            }
            catch (Exception ex) { errors.Add(new BulkRowError(i, ex.Message)); }
        }
        await repo.SaveChangesAsync(ct);
        return new BulkImportResult(created, errors.Count, errors);
    }
}
