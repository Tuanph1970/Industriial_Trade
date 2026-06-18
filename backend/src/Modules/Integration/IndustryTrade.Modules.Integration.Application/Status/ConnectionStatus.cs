using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Integration.Application.Services;
using IndustryTrade.Modules.Integration.Domain.Status;

namespace IndustryTrade.Modules.Integration.Application.Status;

public sealed record ComponentStatusDto(string Component, int Level, bool Healthy, string? Detail);
public sealed record ConnectionStatusDto(bool Healthy, IReadOnlyList<ComponentStatusDto> Components);
public sealed record ConnectionCheckDto(string Component, int Level, bool Healthy, string? Detail, DateTime CheckedAtUtc);

/// <summary>Probes system-level dependencies (design F2, level 1).</summary>
public interface IConnectionProbe
{
    Task<bool> PingDatabaseAsync(CancellationToken ct);
}

public interface IConnectionStatusStore
{
    Task RecordAsync(IEnumerable<ConnectionStatusCheck> checks, CancellationToken ct);
    Task<IReadOnlyList<ConnectionStatusCheck>> ListAsync(Specification<ConnectionStatusCheck> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<ConnectionStatusCheck> spec, CancellationToken ct);
}

/// <summary>
/// Live connection-status report: probes the database (level 1) and reports each published
/// data-sharing service (level 2), recording a history snapshot (retained ≥ 3 months). Design F2.
/// </summary>
public sealed record GetConnectionStatusQuery : IQuery<ConnectionStatusDto>, IPermissionAuthorized
{
    public string RequiredPermission => IntegrationPermissions.Read;
}

public sealed class GetConnectionStatusHandler(
    IConnectionProbe probe,
    IDataSharingServiceRepository services,
    IConnectionStatusStore store) : IQueryHandler<GetConnectionStatusQuery, ConnectionStatusDto>
{
    public async Task<Result<ConnectionStatusDto>> Handle(GetConnectionStatusQuery query, CancellationToken ct)
    {
        var dbOk = await probe.PingDatabaseAsync(ct);
        var components = new List<ComponentStatusDto>
        {
            new("database", 1, dbOk, dbOk ? "reachable" : "unreachable")
        };

        foreach (var svc in await services.GetPublishedAsync(ct))
            components.Add(new ComponentStatusDto(svc.Code, 2, true, $"Published ({svc.Direction})"));

        var overall = components.All(c => c.Healthy);

        await store.RecordAsync(
            components.Select(c => ConnectionStatusCheck.Record(c.Component, c.Level, c.Healthy, c.Detail)), ct);

        return new ConnectionStatusDto(overall, components);
    }
}

public sealed record GetConnectionStatusHistoryQuery(PageRequest Page)
    : IQuery<PagedResult<ConnectionCheckDto>>, IPermissionAuthorized
{
    public string RequiredPermission => IntegrationPermissions.Read;
}

public sealed class ConnectionHistorySpec : Specification<ConnectionStatusCheck>
{
    public ConnectionHistorySpec(PageRequest request, bool forCount = false)
    {
        if (!forCount)
        {
            ApplyOrderByDescending(c => c.CheckedAtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed class GetConnectionStatusHistoryHandler(IConnectionStatusStore store)
    : IQueryHandler<GetConnectionStatusHistoryQuery, PagedResult<ConnectionCheckDto>>
{
    public async Task<Result<PagedResult<ConnectionCheckDto>>> Handle(GetConnectionStatusHistoryQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await store.ListAsync(new ConnectionHistorySpec(page), ct);
        var total = await store.CountAsync(new ConnectionHistorySpec(page, forCount: true), ct);
        var dtos = items
            .Select(c => new ConnectionCheckDto(c.Component, c.Level, c.Healthy, c.Detail, c.CheckedAtUtc))
            .ToList();
        return new PagedResult<ConnectionCheckDto>(dtos, total, page.NormalizedPage, page.NormalizedPageSize);
    }
}
