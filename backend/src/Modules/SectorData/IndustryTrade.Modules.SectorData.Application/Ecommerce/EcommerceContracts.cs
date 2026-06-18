using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;

namespace IndustryTrade.Modules.SectorData.Application.Ecommerce;

public sealed record EcommerceParticipantDto(
    Guid Id, string TaxCode, string BusinessName, Guid OrgUnitId, string[] Platforms, string? MainGoods)
{
    public static EcommerceParticipantDto FromEntity(EcommerceParticipant e) =>
        new(e.Id, e.TaxCode, e.BusinessName, e.OrgUnitId, e.Platforms, e.MainGoods);
}

public interface IEcommerceParticipantRepository
{
    Task<bool> ExistsByTaxCodeAsync(string taxCode, CancellationToken ct);
    Task<IReadOnlyList<EcommerceParticipant>> ListAsync(Specification<EcommerceParticipant> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<EcommerceParticipant> spec, CancellationToken ct);
    Task AddAsync(EcommerceParticipant participant, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class EcommerceSearchSpec : Specification<EcommerceParticipant>
{
    public EcommerceSearchSpec(PageRequest request, Guid[]? scopeUnitIds, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(e => e.TaxCode.ToLower().Contains(kw) || e.BusinessName.ToLower().Contains(kw));
        }
        if (scopeUnitIds is not null)
            Where(e => scopeUnitIds.Contains(e.OrgUnitId));

        if (!forCount)
        {
            ApplyOrderBy(e => e.BusinessName);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
