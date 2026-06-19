namespace IndustryTrade.Modules.SectorData.Application;

public static class SectorPermissions
{
    public const string ObservationsRead = "sector.observations.read";
    public const string ObservationsManage = "sector.observations.manage";
    public const string ObservationsSubmit = "sector.observations.submit";   // commune sends for approval
    public const string ObservationsApprove = "sector.observations.approve"; // specialist/leader approves or returns
    public const string ClustersRead = "sector.clusters.read";
    public const string ClustersManage = "sector.clusters.manage";
    public const string ViolationsRead = "sector.violations.read";
    public const string ViolationsManage = "sector.violations.manage";
    public const string PetrolRead = "sector.petrol.read";
    public const string PetrolManage = "sector.petrol.manage";
    public const string CommerceRead = "sector.commerce.read";
    public const string CommerceManage = "sector.commerce.manage";
    public const string EcommerceRead = "sector.ecommerce.read";
    public const string EcommerceManage = "sector.ecommerce.manage";

    /// <summary>All sector permission codes — convenient for seeding/granting in bulk.</summary>
    public static readonly string[] All =
    [
        ObservationsRead, ObservationsManage, ObservationsSubmit, ObservationsApprove,
        ClustersRead, ClustersManage,
        ViolationsRead, ViolationsManage, PetrolRead, PetrolManage,
        CommerceRead, CommerceManage, EcommerceRead, EcommerceManage
    ];
}
