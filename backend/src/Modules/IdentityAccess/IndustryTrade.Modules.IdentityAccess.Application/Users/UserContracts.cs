using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Users;

namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

public sealed record UserDto(
    Guid Id, string UserName, string? FullName, string? Email,
    Guid? OrgUnitId, Guid[] RoleIds, bool IsActive)
{
    public static UserDto FromEntity(UserAccount u) =>
        new(u.Id, u.UserName, u.FullName, u.Email, u.OrgUnitId, u.RoleIds, u.IsActive);
}

public interface IUserRepository
{
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<UserAccount?> GetByUserNameAsync(string userName, CancellationToken ct);
    Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct);
    Task<IReadOnlyList<UserAccount>> ListAsync(Specification<UserAccount> spec, CancellationToken ct);
    Task<int> CountAsync(Specification<UserAccount> spec, CancellationToken ct);
    Task AddAsync(UserAccount user, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class UserSearchSpec : Specification<UserAccount>
{
    public UserSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(u => u.UserName.ToLower().Contains(kw)
                       || (u.FullName != null && u.FullName.ToLower().Contains(kw)));
        }

        if (!forCount)
        {
            ApplyOrderBy(u => u.UserName);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
