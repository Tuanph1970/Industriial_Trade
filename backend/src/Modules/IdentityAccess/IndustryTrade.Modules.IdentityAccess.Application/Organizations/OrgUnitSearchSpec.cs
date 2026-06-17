using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Organizations;

/// <summary>
/// Example of the reusable Specification pattern behind every list/search/paginate use case.
/// Pass <paramref name="forCount"/> = true to get the same filter without paging, for the total count.
/// </summary>
public sealed class OrgUnitSearchSpec : Specification<OrgUnit>
{
    public OrgUnitSearchSpec(PageRequest request, bool forCount = false)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim().ToLower();
            Where(u => u.Code.ToLower().Contains(kw) || u.Name.ToLower().Contains(kw));
        }

        if (!forCount)
        {
            ApplyOrderBy(u => u.Path);
            ApplyPaging(request.Skip, request.NormalizedPageSize);
        }
    }
}
