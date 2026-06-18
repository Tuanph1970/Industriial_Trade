using FluentValidation;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Domain;
using IndustryTrade.Modules.SectorData.Domain.Violations;

namespace IndustryTrade.Modules.SectorData.Application.Violations;

public sealed record CreateViolationCommand(
    string CaseNo, ViolationGroup Group, Guid OrgUnitId, string BusinessName,
    DateOnly InspectedOn, string ViolationContent)
    : ICommand<Guid>, IPermissionAuthorized
{
    public string RequiredPermission => SectorPermissions.ViolationsManage;
}

public sealed class CreateViolationValidator : AbstractValidator<CreateViolationCommand>
{
    public CreateViolationValidator()
    {
        RuleFor(x => x.CaseNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Group).IsInEnum();
        RuleFor(x => x.OrgUnitId).NotEmpty();
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ViolationContent).NotEmpty();
    }
}

public sealed class CreateViolationHandler(IViolationRepository repository)
    : ICommandHandler<CreateViolationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateViolationCommand command, CancellationToken ct)
    {
        if (await repository.ExistsByCaseNoAsync(command.CaseNo, ct))
            return Result.Failure<Guid>(Error.Conflict($"Case number '{command.CaseNo}' already exists."));

        var violation = MarketViolationCase.Create(command.CaseNo, command.Group, command.OrgUnitId,
            command.BusinessName, command.InspectedOn, command.ViolationContent);
        await repository.AddAsync(violation, ct);
        await repository.SaveChangesAsync(ct);
        return violation.Id;
    }
}
