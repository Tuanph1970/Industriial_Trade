using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.BuildingBlocks.Infrastructure.Persistence;
using IndustryTrade.Modules.Notifications.Application;
using IndustryTrade.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace IndustryTrade.Modules.Notifications.Infrastructure.Persistence;

internal sealed class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Notification>> ListAsync(Specification<Notification> spec, CancellationToken ct) =>
        await SpecificationEvaluator.Apply(db.Notifications.AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(Specification<Notification> spec, CancellationToken ct) =>
        SpecificationEvaluator.Apply(db.Notifications.AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct) => await db.Notifications.AddAsync(notification, ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
