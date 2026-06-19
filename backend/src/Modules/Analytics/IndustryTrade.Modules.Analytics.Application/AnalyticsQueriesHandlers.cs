using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Analytics.Application;

internal static class Scope
{
    public static Guid[]? For(ICurrentUser user) => user.IsSuperAdmin ? null : user.DataScopeUnitIds.ToArray();
}

public sealed record GetDashboardQuery : IQuery<DashboardDto>, IPermissionAuthorized
{
    public string RequiredPermission => AnalyticsPermissions.Read;
}

public sealed class GetDashboardHandler(IAnalyticsQueries queries, ICurrentUser currentUser)
    : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery query, CancellationToken ct) =>
        await queries.GetDashboardAsync(Scope.For(currentUser), ct);
}

public sealed record GetViolationsSummaryQuery : IQuery<IReadOnlyList<ViolationSummaryRow>>, IPermissionAuthorized
{
    public string RequiredPermission => AnalyticsPermissions.Read;
}

public sealed class GetViolationsSummaryHandler(IAnalyticsQueries queries, ICurrentUser currentUser)
    : IQueryHandler<GetViolationsSummaryQuery, IReadOnlyList<ViolationSummaryRow>>
{
    public async Task<Result<IReadOnlyList<ViolationSummaryRow>>> Handle(GetViolationsSummaryQuery query, CancellationToken ct) =>
        Result.Success(await queries.GetViolationsSummaryAsync(Scope.For(currentUser), ct));
}

public sealed record GetReportingSummaryQuery : IQuery<IReadOnlyList<StateCount>>, IPermissionAuthorized
{
    public string RequiredPermission => AnalyticsPermissions.Read;
}

public sealed class GetReportingSummaryHandler(IAnalyticsQueries queries, ICurrentUser currentUser)
    : IQueryHandler<GetReportingSummaryQuery, IReadOnlyList<StateCount>>
{
    public async Task<Result<IReadOnlyList<StateCount>>> Handle(GetReportingSummaryQuery query, CancellationToken ct) =>
        Result.Success(await queries.GetReportingSummaryAsync(Scope.For(currentUser), ct));
}

public sealed record GetObservationsBySectorQuery : IQuery<IReadOnlyList<SectorObservationRow>>, IPermissionAuthorized
{
    public string RequiredPermission => AnalyticsPermissions.Read;
}

public sealed class GetObservationsBySectorHandler(IAnalyticsQueries queries, ICurrentUser currentUser)
    : IQueryHandler<GetObservationsBySectorQuery, IReadOnlyList<SectorObservationRow>>
{
    public async Task<Result<IReadOnlyList<SectorObservationRow>>> Handle(GetObservationsBySectorQuery query, CancellationToken ct) =>
        Result.Success(await queries.GetObservationsBySectorAsync(Scope.For(currentUser), ct));
}

public sealed record GetCommerceByTypeQuery : IQuery<IReadOnlyList<CommerceTypeRow>>, IPermissionAuthorized
{
    public string RequiredPermission => AnalyticsPermissions.Read;
}

public sealed class GetCommerceByTypeHandler(IAnalyticsQueries queries, ICurrentUser currentUser)
    : IQueryHandler<GetCommerceByTypeQuery, IReadOnlyList<CommerceTypeRow>>
{
    public async Task<Result<IReadOnlyList<CommerceTypeRow>>> Handle(GetCommerceByTypeQuery query, CancellationToken ct) =>
        Result.Success(await queries.GetCommerceByTypeAsync(Scope.For(currentUser), ct));
}
