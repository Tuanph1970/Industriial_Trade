using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.Catalog.Domain.Indicators;

namespace IndustryTrade.Modules.Catalog.Application.Indicators;

public static class CatalogPermissions
{
    public const string IndicatorsRead = "catalog.indicators.read";
    public const string IndicatorsManage = "catalog.indicators.manage";
}

public sealed record IndicatorDto(
    Guid Id, string Code, string Name, string Unit,
    IndicatorDataType DataType, IndustrySector Sector,
    DateOnly EffectiveFrom, DateOnly? RetiredAt, int Version, bool IsActive)
{
    public static IndicatorDto FromEntity(Indicator i) =>
        new(i.Id, i.Code, i.Name, i.Unit, i.DataType, i.Sector, i.EffectiveFrom, i.RetiredAt, i.Version, i.IsActive);
}

public interface IIndicatorRepository
{
    Task<Indicator?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Indicator>> ListAsync(Specification<Indicator> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<Indicator> spec, CancellationToken ct);
    Task AddAsync(Indicator indicator, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class IndicatorSearchSpec : Specification<Indicator>
{
    public IndicatorSearchSpec(PageRequest request, IndustrySector? sector = null, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(i => i.Code.ToLower().Contains(kw) || i.Name.ToLower().Contains(kw));
        }
        if (sector is { } s)
            Where(i => i.Sector == s);

        if (!forCount)
        {
            ApplyOrderBy(i => i.Code);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
