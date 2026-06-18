using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>Persistence port for the OrgUnit aggregate (implemented in Infrastructure).</summary>
public interface IOrgUnitRepository
{
    Task<OrgUnit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> HasChildrenAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<OrgUnit>> ListAsync(Specification<OrgUnit> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<OrgUnit> spec, CancellationToken ct);
    Task AddAsync(OrgUnit unit, CancellationToken ct);
    void Remove(OrgUnit unit);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
