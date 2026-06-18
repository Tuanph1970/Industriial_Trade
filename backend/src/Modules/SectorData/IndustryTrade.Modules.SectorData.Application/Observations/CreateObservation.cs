using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.Observations;

namespace IndustryTrade.Modules.SectorData.Application.Observations;

public sealed record CreateObservationCommand(
    Guid IndicatorId, Guid OrgUnitId, int PeriodYear, int? PeriodMonth,
    decimal? Value, string? ValueText, string? Source)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ObservationsManage;
}

public sealed class CreateObservationValidator : AbstractValidator<CreateObservationCommand>
{
    public CreateObservationValidator()
    {
        RuleFor(x => x.IndicatorId).NotEmpty();
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.PeriodYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12).When(x => x.PeriodMonth.HasValue);
    }
}

public sealed class CreateObservationHandler(IObservationRepository repository)
    : ICommandHandler<CreateObservationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateObservationCommand command, CancellationToken ct)
    {
        var observation = IndicatorObservation.Create(
            command.IndicatorId, command.OrgUnitId, command.PeriodYear, command.PeriodMonth,
            command.Value, command.ValueText, command.Source);
        await repository.AddAsync(observation, ct);
        await repository.SaveChangesAsync(ct);
        return observation.Id;
    }
}
