using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Domain.Indicators;

namespace IndustryTrade.Modules.Catalog.Application.Indicators;

// ── Update ────────────────────────────────────────────────────────────────
public sealed record UpdateIndicatorCommand(
    Guid Id, string Name, string Unit, IndicatorDataType DataType, IndustrySector Sector)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.IndicatorsManage;
}

public sealed class UpdateIndicatorValidator : AbstractValidator<UpdateIndicatorCommand>
{
    public UpdateIndicatorValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DataType).IsInEnum();
        RuleFor(x => x.Sector).IsInEnum();
    }
}

public sealed class UpdateIndicatorHandler(IIndicatorRepository repository) : ICommandHandler<UpdateIndicatorCommand>
{
    public async Task<Result> Handle(UpdateIndicatorCommand command, CancellationToken ct)
    {
        var indicator = await repository.GetByIdAsync(command.Id, ct);
        if (indicator is null)
            return Result.Failure(Error.NotFound("Indicator"));

        indicator.Update(command.Name, command.Unit, command.DataType, command.Sector);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete ────────────────────────────────────────────────────────────────
public sealed record DeleteIndicatorCommand(Guid Id) : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.IndicatorsManage;
}

public sealed class DeleteIndicatorHandler(IIndicatorRepository repository) : ICommandHandler<DeleteIndicatorCommand>
{
    public async Task<Result> Handle(DeleteIndicatorCommand command, CancellationToken ct)
    {
        var indicator = await repository.GetByIdAsync(command.Id, ct);
        if (indicator is null)
            return Result.Failure(Error.NotFound("Indicator"));

        repository.Remove(indicator);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Detail ────────────────────────────────────────────────────────────────
public sealed record GetIndicatorByIdQuery(Guid Id) : IQuery<IndicatorDto>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.IndicatorsRead;
}

public sealed class GetIndicatorByIdHandler(IIndicatorRepository repository) : IQueryHandler<GetIndicatorByIdQuery, IndicatorDto>
{
    public async Task<Result<IndicatorDto>> Handle(GetIndicatorByIdQuery query, CancellationToken ct)
    {
        var indicator = await repository.GetByIdAsync(query.Id, ct);
        return indicator is null
            ? Result.Failure<IndicatorDto>(Error.NotFound("Indicator"))
            : IndicatorDto.FromEntity(indicator);
    }
}
