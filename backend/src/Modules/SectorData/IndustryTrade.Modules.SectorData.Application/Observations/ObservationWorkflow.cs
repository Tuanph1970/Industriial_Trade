using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Application.Observations;

/// <summary>Approval transitions a caller can request on an observation.</summary>
public enum ObservationAction { Submit, Approve, Return }

/// <summary>
/// One command for every observation state-machine transition. The required permission is computed
/// from the action so the AuthorizationBehavior enforces the right role (commune submits;
/// specialist/leader approves or returns). Data-scoped by the observation's org unit; audited (G1).
/// </summary>
public sealed record ObservationActionCommand(Guid Id, ObservationAction Action)
    : ICommand, IPermissionAuthorized
{
    public string RequiredPermission => Action switch
    {
        ObservationAction.Submit => SectorPermissions.ObservationsSubmit,
        ObservationAction.Approve or ObservationAction.Return => SectorPermissions.ObservationsApprove,
        _ => SectorPermissions.ObservationsRead,
    };
}

public sealed class ObservationActionHandler(IObservationRepository repository, ICurrentUser currentUser)
    : ICommandHandler<ObservationActionCommand>
{
    public async Task<Result> Handle(ObservationActionCommand command, CancellationToken ct)
    {
        var observation = await repository.GetByIdAsync(command.Id, ct);
        if (observation is null)
            return Result.Failure(Error.NotFound("Observation"));

        if (!currentUser.IsSuperAdmin && !currentUser.DataScopeUnitIds.Contains(observation.OrgUnitId))
            return Result.Failure(Error.Forbidden("Observation is outside your data scope."));

        try
        {
            switch (command.Action)
            {
                case ObservationAction.Submit: observation.Submit(); break;
                case ObservationAction.Approve: observation.Approve(); break;
                case ObservationAction.Return: observation.ReturnToDraft(); break;
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
