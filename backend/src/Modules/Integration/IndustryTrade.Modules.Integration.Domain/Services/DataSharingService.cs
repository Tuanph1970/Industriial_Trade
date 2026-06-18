using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.Integration.Domain.Services;

public enum ServiceDirection { Provide = 1, Consume = 2 } // cung cấp / khai thác
public enum ServiceStatus { Registered = 1, Published = 2, Revoked = 3 }

/// <summary>
/// A data-sharing service registered with the provincial LGSP / national NDXP (Decree 47/2020).
/// Lifecycle: Registered → Published → Revoked (docs/design/03 §8).
/// </summary>
public sealed class DataSharingService : AggregateRoot<Guid>, IAuditable
{
    private DataSharingService() { } // EF

    private DataSharingService(Guid id, string code, string name, ServiceDirection direction,
        string? endpointUrl, string? description) : base(id)
    {
        Code = code;
        Name = name;
        Direction = direction;
        EndpointUrl = endpointUrl;
        Description = description;
        Status = ServiceStatus.Registered;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public ServiceDirection Direction { get; private set; }
    public string? EndpointUrl { get; private set; }
    public string? Description { get; private set; }
    public ServiceStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static DataSharingService Create(string code, string name, ServiceDirection direction,
        string? endpointUrl, string? description)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Service code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Service name is required.", nameof(name));

        return new DataSharingService(Guid.NewGuid(), code.Trim(), name.Trim(), direction,
            endpointUrl?.Trim(), description?.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Publish()
    {
        if (Status == ServiceStatus.Revoked)
            throw new BusinessRuleException("A revoked service cannot be published.");
        Status = ServiceStatus.Published;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (Status != ServiceStatus.Published)
            throw new BusinessRuleException("Only a published service can be revoked.");
        Status = ServiceStatus.Revoked;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
