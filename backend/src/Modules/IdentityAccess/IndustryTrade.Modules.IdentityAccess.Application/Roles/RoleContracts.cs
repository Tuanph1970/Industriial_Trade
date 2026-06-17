using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Roles;

namespace IndustryTrade.Modules.IdentityAccess.Application.Roles;

public sealed record RoleDto(Guid Id, string Code, string Name, string[] Permissions, bool IsActive)
{
    public static RoleDto FromEntity(Role r) => new(r.Id, r.Code, r.Name, r.Permissions, r.IsActive);
}

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Role>> ListAsync(Specification<Role> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<Role> spec, CancellationToken ct);
    Task AddAsync(Role role, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class RoleSearchSpec : Specification<Role>
{
    public RoleSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(r => r.Code.ToLower().Contains(kw) || r.Name.ToLower().Contains(kw));
        }

        if (!forCount)
        {
            ApplyOrderBy(r => r.Name);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
