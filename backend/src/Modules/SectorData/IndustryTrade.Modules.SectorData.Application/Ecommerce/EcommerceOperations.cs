using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.Ecommerce;

namespace IndustryTrade.Modules.SectorData.Application.Ecommerce;

public sealed record CreateEcommerceParticipantCommand(
    string TaxCode, string BusinessName, Guid OrgUnitId, string[] Platforms, string? MainGoods)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.EcommerceManage;
}

public sealed class CreateEcommerceParticipantValidator : AbstractValidator<CreateEcommerceParticipantCommand>
{
    public CreateEcommerceParticipantValidator()
    {
        RuleFor(x => x.TaxCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.OrgUnitId).NotEmpty();
    }
}

public sealed class CreateEcommerceParticipantHandler(IEcommerceParticipantRepository repository)
    : ICommandHandler<CreateEcommerceParticipantCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEcommerceParticipantCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByTaxCodeAsync(command.TaxCode, ct))
            return Result.Failure<Guid>(Error.Conflict($"Tax code '{command.TaxCode}' already exists."));

        var participant = EcommerceParticipant.Create(
            command.TaxCode, command.BusinessName, command.OrgUnitId, command.Platforms ?? [], command.MainGoods);
        await repository.AddAsync(participant, ct);
        await repository.SaveChangesAsync(ct);
        return participant.Id;
    }
}

public sealed record GetEcommerceParticipantsQuery(PageRequest Page)
    : IQuery<PagedResult<EcommerceParticipantDto>>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.EcommerceRead;
}

public sealed class GetEcommerceParticipantsHandler(IEcommerceParticipantRepository repository, ICurrentUser currentUser)
    : IQueryHandler<GetEcommerceParticipantsQuery, PagedResult<EcommerceParticipantDto>>
{
    public async Task<Result<PagedResult<EcommerceParticipantDto>>> Handle(GetEcommerceParticipantsQuery query, CancellationToken ct)
    {
        var page = query.Page;
        var scope = currentUser.IsSuperAdmin ? null : currentUser.DataScopeUnitIds.ToArray();
        var items = await repository.ListAsync(new EcommerceSearchSpec(page, scope), ct);
        var total = await repository.CountAsync(new EcommerceSearchSpec(page, scope, forCount: true), ct);
        return new PagedResult<EcommerceParticipantDto>(items.Select(EcommerceParticipantDto.FromEntity).ToList(), total,
            page.NormalizedPage, page.NormalizedPageSize);
    }
}
