using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.Catalog.Domain.Indicators;

namespace IndustryTrade.Modules.Catalog.Application.Indicators;

public sealed record CreateIndicatorCommand(
    string Code, string Name, string Unit,
    IndicatorDataType DataType, IndustrySector Sector, DateOnly EffectiveFrom)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => CatalogPermissions.IndicatorsManage;
}

public sealed class CreateIndicatorValidator : AbstractValidator<CreateIndicatorCommand>
{
    public CreateIndicatorValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DataType).IsInEnum();
        RuleFor(x => x.Sector).IsInEnum();
    }
}

public sealed class CreateIndicatorHandler(IIndicatorRepository repository)
    : ICommandHandler<CreateIndicatorCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateIndicatorCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCodeAsync(command.Code, ct))
            return Result.Failure<Guid>(Error.Conflict($"Indicator code '{command.Code}' already exists."));

        var indicator = Indicator.Create(
            command.Code, command.Name, command.Unit, command.DataType, command.Sector, command.EffectiveFrom);
        await repository.AddAsync(indicator, ct);
        await repository.SaveChangesAsync(ct);
        return indicator.Id;
    }
}
