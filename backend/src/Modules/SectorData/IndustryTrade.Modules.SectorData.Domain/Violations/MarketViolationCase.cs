using IndustryTrade.BuildingBlocks.Domain;

namespace IndustryTrade.Modules.SectorData.Domain.Violations;

/// <summary>The two record groups from the requirements (UC-18/19, 28/29).</summary>
public enum ViolationGroup
{
    /// <summary>Hàng cấm, nhập lậu, hàng giả, hàng nhái, hàng kém chất lượng.</summary>
    ProhibitedAndCounterfeit = 1,
    /// <summary>Vệ sinh, an toàn thực phẩm.</summary>
    FoodSafety = 2
}

public enum ViolationStatus { Reported = 1, UnderHandling = 2, Resolved = 3 }

/// <summary>
/// A business-violation case file (hồ sơ vi phạm trong kinh doanh) — the market-surveillance
/// records aggregate (docs/design/03 §4a "MarketViolationCase"). Owned by an org unit, so it is
/// data-scoped by unit id like other Sector entities.
/// </summary>
public sealed class MarketViolationCase : AggregateRoot<Guid>, IAuditable
{
    private MarketViolationCase() { } // EF

    private MarketViolationCase(Guid id, string caseNo, ViolationGroup group, Guid orgUnitId,
        string businessName, DateOnly inspectedOn, string violationContent) : base(id)
    {
        CaseNo = caseNo;
        Group = group;
        OrgUnitId = orgUnitId;
        BusinessName = businessName;
        InspectedOn = inspectedOn;
        ViolationContent = violationContent;
        Status = ViolationStatus.Reported;
    }

    public string CaseNo { get; private set; } = default!;
    public ViolationGroup Group { get; private set; }
    public Guid OrgUnitId { get; private set; }
    public string BusinessName { get; private set; } = default!;
    public DateOnly InspectedOn { get; private set; }
    public string ViolationContent { get; private set; } = default!;
    public string? SanctionContent { get; private set; }
    public decimal? FineAmount { get; private set; }
    public ViolationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public static MarketViolationCase Create(string caseNo, ViolationGroup group, Guid orgUnitId,
        string businessName, DateOnly inspectedOn, string violationContent)
    {
        if (string.IsNullOrWhiteSpace(caseNo)) throw new ArgumentException("Case number is required.", nameof(caseNo));
        if (orgUnitId == Guid.Empty) throw new ArgumentException("Org unit is required.", nameof(orgUnitId));
        if (string.IsNullOrWhiteSpace(businessName)) throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(violationContent)) throw new ArgumentException("Violation content is required.", nameof(violationContent));

        return new MarketViolationCase(Guid.NewGuid(), caseNo.Trim(), group, orgUnitId,
            businessName.Trim(), inspectedOn, violationContent.Trim())
        {
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>Edits the descriptive and resolution fields of the case (admin/correction edit).</summary>
    public void Update(ViolationGroup group, string businessName, DateOnly inspectedOn,
        string violationContent, string? sanctionContent, decimal? fineAmount, ViolationStatus status)
    {
        if (string.IsNullOrWhiteSpace(businessName)) throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(violationContent)) throw new ArgumentException("Violation content is required.", nameof(violationContent));
        if (fineAmount is < 0) throw new ArgumentOutOfRangeException(nameof(fineAmount));

        Group = group;
        BusinessName = businessName.Trim();
        InspectedOn = inspectedOn;
        ViolationContent = violationContent.Trim();
        SanctionContent = sanctionContent?.Trim();
        FineAmount = fineAmount;
        Status = status;
        Touch();
    }

    public void StartHandling()
    {
        Status = ViolationStatus.UnderHandling;
        Touch();
    }

    public void Resolve(string sanctionContent, decimal? fineAmount)
    {
        if (string.IsNullOrWhiteSpace(sanctionContent))
            throw new ArgumentException("Sanction content is required to resolve a case.", nameof(sanctionContent));
        if (fineAmount is < 0)
            throw new ArgumentOutOfRangeException(nameof(fineAmount));

        SanctionContent = sanctionContent.Trim();
        FineAmount = fineAmount;
        Status = ViolationStatus.Resolved;
        Touch();
    }

    private void Touch() => ModifiedAtUtc = DateTime.UtcNow;
}
