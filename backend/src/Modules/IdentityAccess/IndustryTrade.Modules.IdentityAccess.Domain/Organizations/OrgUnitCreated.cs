using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Domain.Organizations;

public sealed record OrgUnitCreated(Guid OrgUnitId, string Code, string Name, Guid? ParentId) : DomainEvent;
