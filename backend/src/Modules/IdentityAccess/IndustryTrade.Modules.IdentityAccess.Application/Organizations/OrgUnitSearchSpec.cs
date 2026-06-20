using System.Linq.Expressions;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>
/// The reusable Specification behind the list/search/paginate use case, now also enforcing
/// <b>data-scope</b>: when <paramref name="scopeUnitIds"/> is non-null the result is limited to those
/// org-unit ids (the caller's unit + its subtree, already resolved via the ltree <c>&lt;@</c> query).
/// Pass null for super-admin (no restriction); an empty array denies all. <paramref name="forCount"/>
/// drops paging for the total.
/// </summary>
public sealed class OrgUnitSearchSpec : Specification<OrgUnit>
{
    public OrgUnitSearchSpec(
        PageRequest request,
        Guid[]? scopeUnitIds = null,
        bool forCount = false)
    {
        Expression<Func<OrgUnit, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            predicate = u => u.Code.ToLower().Contains(kw) || u.Name.ToLower().Contains(kw);
        }

        if (scopeUnitIds is not null)
        {
            Expression<Func<OrgUnit, bool>> scope = u => scopeUnitIds.Contains(u.Id);
            predicate = predicate is null ? scope : predicate.And(scope);
        }

        if (predicate is not null)
            Where(predicate);

        if (!forCount)
        {
            ApplyOrderBy(u => u.Path);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
