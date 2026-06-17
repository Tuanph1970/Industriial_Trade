using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.IdentityAccess.Domain.Roles;

/// <summary>A named bundle of function-scope permission codes (docs/design/03 §2).</summary>
public sealed class Role : AggregateRoot<Guid>, IAuditable
{
    private Role() { } // EF

    private Role(Guid id, string code, string name, string[] permissions) : base(id)
    {
        Code = code;
        Name = name;
        Permissions = permissions;
    }

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string[] Permissions { get; private set; } = [];
    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static Role Create(string code, string name, IEnumerable<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Role code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));

        return new Role(Guid.NewGuid(), code.Trim(), name.Trim(), Normalize(permissions))
        {
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, IEnumerable<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));
        Name = name.Trim();
        Permissions = Normalize(permissions);
        ModifiedAtUtc = DateTime.UtcNow;
    }

    private static string[] Normalize(IEnumerable<string> permissions) =>
        permissions.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToArray();
}
