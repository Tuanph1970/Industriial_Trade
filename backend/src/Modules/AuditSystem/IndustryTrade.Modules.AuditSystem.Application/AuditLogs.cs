using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.AuditSystem.Domain;

namespace IndustryTrade.Modules.AuditSystem.Application;

public static class AuditPermissions
{
    public const string Read = "audit.read";
}

public sealed record AuditLogDto(
    Guid Id, string? Actor, string Action, string Payload, bool Success, string? Error, DateTime AtUtc)
{
    public static AuditLogDto FromEntity(AuditLogEntry e) =>
        new(e.Id, e.Actor, e.Action, e.Payload, e.Success, e.Error, e.AtUtc);
}

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLogEntry>> ListAsync(Specification<AuditLogEntry> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<AuditLogEntry> spec, CancellationToken ct);
}

/// <summary>Search/browse the log by actor, action and date range (design G1).</summary>
public sealed class AuditSearchSpec : Specification<AuditLogEntry>
{
    public AuditSearchSpec(PageRequest request, string? actor, string? action, DateTime? fromUtc, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(actor))
        {
            var a = actor.Trim().ToLower();
            Where(e => e.Actor != null && e.Actor.ToLower().Contains(a));
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            var ac = action.Trim().ToLower();
            Where(e => e.Action.ToLower().Contains(ac));
        }
        if (fromUtc is { } f)
            Where(e => e.AtUtc >= f);

        if (!forCount)
        {
            ApplyOrderByDescending(e => e.AtUtc);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}

public sealed record GetAuditLogsQuery(PageRequest Page, string? Actor, string? Action, DateTime? FromUtc)
    : IQuery<PagedResult<AuditLogDto>>, IPermissionAuthorized
{
    public string RequiredPermission => AuditPermissions.Read;
}

public sealed class GetAuditLogsHandler(IAuditLogRepository repository)
    : IQueryHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(GetAuditLogsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var spec = new AuditSearchSpec(page, query.Actor, query.Action, query.FromUtc);
        var countSpec = new AuditSearchSpec(page, query.Actor, query.Action, query.FromUtc, forCount: true);
        var items = await repository.ListAsync(spec, ct);
        var total = await repository.CountAsync(countSpec, ct);
        return new PagedResult<AuditLogDto>(items.Select(AuditLogDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
