using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Specifications;
using IndustryTrade.Modules.Notifications.Application;
using IndustryTrade.Modules.Notifications.Domain;
using IndustryTrade.Modules.Reporting.Domain.Submissions;
using Xunit;

namespace IndustryTrade.Modules.Notifications.Tests;

public class ReportStateChangedNotificationHandlerTests
{
    [Fact]
    public async Task Creates_a_notification_from_a_report_state_change()
    {
        var repo = new InMemoryNotificationRepository();
        var handler = new ReportStateChangedNotificationHandler(repo);
        var submissionId = Guid.NewGuid();

        var evt = new ReportStateChanged(submissionId, ReportState.Draft, ReportState.Submitted, "Submit");
        await handler.Handle(new DomainEventNotification<ReportStateChanged>(evt), CancellationToken.None);

        repo.Items.Should().ContainSingle();
        var n = repo.Items[0];
        n.Category.Should().Be("reporting");
        n.RefId.Should().Be(submissionId.ToString());
        n.IsRead.Should().BeFalse();
    }

    private sealed class InMemoryNotificationRepository : INotificationRepository
    {
        public List<Notification> Items { get; } = new();
        public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Notification>> ListAsync(Specification<Notification> spec, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Notification>>(Items);
        public Task<int> CountAsync(Specification<Notification> spec, CancellationToken ct) => Task.FromResult(Items.Count);
        public Task<IReadOnlyList<Notification>> ListUnreadAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<Notification>>(Items.Where(x => !x.IsRead).ToList());
        public Task AddAsync(Notification notification, CancellationToken ct) { Items.Add(notification); return Task.CompletedTask; }
        public Task<int> SaveChangesAsync(CancellationToken ct) => Task.FromResult(0);
    }
}
