using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Domain.Ecommerce;

/// <summary>
/// A business participating in e-commerce platforms on the province's territory (UC-15/25).
/// No geometry; tracks the platforms it sells on and its main goods.
/// </summary>
public sealed class EcommerceParticipant : AggregateRoot<Guid>, IAuditable
{
    private EcommerceParticipant() { } // EF

    private EcommerceParticipant(Guid id, string taxCode, string businessName, Guid orgUnitId,
        string[] platforms, string? mainGoods) : base(id)
    {
        TaxCode = taxCode;
        BusinessName = businessName;
        OrgUnitId = orgUnitId;
        Platforms = platforms;
        MainGoods = mainGoods;
    }

    public string TaxCode { get; private set; } = default!;
    public string BusinessName { get; private set; } = default!;
    public Guid OrgUnitId { get; private set; }
    public string[] Platforms { get; private set; } = [];
    public string? MainGoods { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static EcommerceParticipant Create(string taxCode, string businessName, Guid orgUnitId,
        IEnumerable<string> platforms, string? mainGoods)
    {
        if (string.IsNullOrWhiteSpace(taxCode)) throw new ArgumentException("Tax code is required.", nameof(taxCode));
        if (string.IsNullOrWhiteSpace(businessName)) throw new ArgumentException("Business name is required.", nameof(businessName));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));

        var normalizedPlatforms = platforms
            .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToArray();

        return new EcommerceParticipant(Guid.NewGuid(), taxCode.Trim(), businessName.Trim(), orgUnitId,
            normalizedPlatforms, mainGoods?.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string businessName, IEnumerable<string> platforms, string? mainGoods)
    {
        if (string.IsNullOrWhiteSpace(businessName)) throw new ArgumentException("Business name is required.", nameof(businessName));

        BusinessName = businessName.Trim();
        Platforms = platforms
            .Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToArray();
        MainGoods = mainGoods?.Trim();
        ModifiedAtUtc = DateTime.UtcNow;
    }
}
