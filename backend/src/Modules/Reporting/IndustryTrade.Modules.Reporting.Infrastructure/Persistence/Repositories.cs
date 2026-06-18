using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Reporting.Application.Campaigns;
using IndustryTrade.Modules.Reporting.Application.Submissions;
using IndustryTrade.Modules.Reporting.Domain.Campaigns;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Reporting.Infrastructure.Persistence;

internal sealed class CampaignRepository(ReportingDbContext db) : ICampaignRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.Campaigns.AnyAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<ReportingCampaign>> ListAsync(Specification<ReportingCampaign> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Campaigns.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<ReportingCampaign> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Campaigns.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(ReportingCampaign campaign, CancellationToken ct) => await db.Campaigns.AddAsync(campaign, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

internal sealed class SubmissionRepository(ReportingDbContext db) : ISubmissionRepository
{
    // Owned History loads automatically with the aggregate.
    public Task<ReportSubmission?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Submissions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ReportSubmission>> ListAsync(Specification<ReportSubmission> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Submissions.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<ReportSubmission> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Submissions.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(ReportSubmission submission, CancellationToken ct) => await db.Submissions.AddAsync(submission, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
