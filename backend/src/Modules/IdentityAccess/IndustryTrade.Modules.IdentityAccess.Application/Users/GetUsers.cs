using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;

namespace IndustryTrade.Modules.IdentityAccess.Application.Users;

public sealed record GetUsersQuery(PageRequest Page) : IQuery<PagedResult<UserDto>>, IPermissionAuthorized
{
    public string RequiredPermission => IdentityPermissions.UsersRead;
}

public sealed class GetUsersHandler(IUserRepository repository)
    : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var items = await repository.ListAsync(new UserSearchSpec(page), ct);
        var total = await repository.CountAsync(new UserSearchSpec(page, forCount: true), ct);
        return new PagedResult<UserDto>(items.Select(UserDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
