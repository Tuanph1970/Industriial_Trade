namespace IndustryTrade.Modules.Reporting.Application;

public static class ReportingPermissions
{
    public const string Read = "reporting.read";
    public const string CampaignsManage = "reporting.campaigns.manage"; // specialist opens periods
    public const string Submit = "reporting.submit";   // commune official creates/sends
    public const string Review = "reporting.review";   // specialist reviews
    public const string Approve = "reporting.approve"; // division leader approves

    public static readonly string[] All = [Read, CampaignsManage, Submit, Review, Approve];
}
