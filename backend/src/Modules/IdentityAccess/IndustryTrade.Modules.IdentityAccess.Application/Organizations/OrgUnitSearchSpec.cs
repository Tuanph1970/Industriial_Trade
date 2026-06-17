using System.Linq.Expressions;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>
/// The reusable Specification behind the list/search/paginate use case, now also enforcing
/// <b>data-scope</b>: when <paramref name="dataScopePaths"/> is non-null the result is limited to
/// the given org-unit paths and their descendants. Pass null for super-admin (no restriction);
/// pass an empty collection to deny all. <paramref name="forCount"/> drops paging for the total.
/// </summary>
public sealed class OrgUnitSearchSpec : Specification<OrgUnit>
{
    public OrgUnitSearchSpec(
        PageRequest request,
        IReadOnlyCollection<string>? dataScopePaths = null,
        bool forCount = false)
    {
        Expression<Func<OrgUnit, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            predicate = u => u.Code.ToLower().Contains(kw) || u.Name.ToLower().Contains(kw);
        }

        if (dataScopePaths is not null)
        {
            Expression<Func<OrgUnit, bool>> scope = _ => false;
            foreach (var path in dataScopePaths)
            {
                var p = path; // capture per iteration
                scope = scope.Or(u => u.Path == p || u.Path.StartsWith(p + "."));
            }
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
