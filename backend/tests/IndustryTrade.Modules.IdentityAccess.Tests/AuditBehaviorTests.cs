using FluentAssertions;
using IndustryTrade.BuildingBlocks.Application.Auditing;
using IndustryTrade.BuildingBlocks.Application.Behaviors;
using IndustryTrade.BuildingBlocks.Application.Messaging;
using IndustryTrade.BuildingBlocks.Application.Security;
using IndustryTrade.BuildingBlocks.Domain;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IndustryTrade.Modules.IdentityAccess.Tests;

public class AuditBehaviorTests
{
    private sealed record FakeCommand(string Code) : ICommand;
    private sealed record FakeQuery : IQuery<int>;

    [Fact]
    public async Task Audits_a_command_with_success_outcome()
    {
        var sink = new FakeAuditSink();
        var behavior = new AuditBehavior<FakeCommand, Result>(sink, new FakeUser(), NullLogger<AuditBehavior<FakeCommand, Result>>.Instance);

        await behavior.Handle(new FakeCommand("X"), () => Task.FromResult(Result.Success()), default);

        sink.Entries.Should().ContainSingle();
        sink.Entries[0].Action.Should().Be(nameof(FakeCommand));
        sink.Entries[0].Success.Should().BeTrue();
        sink.Entries[0].Payload.Should().Contain("\"Code\":\"X\"");
    }

    [Fact]
    public async Task Records_failure_when_result_is_failure()
    {
        var sink = new FakeAuditSink();
        var behavior = new AuditBehavior<FakeCommand, Result>(sink, new FakeUser(), NullLogger<AuditBehavior<FakeCommand, Result>>.Instance);

        await behavior.Handle(new FakeCommand("X"),
            () => Task.FromResult(Result.Failure(Error.Validation("bad"))), default);

        sink.Entries[0].Success.Should().BeFalse();
        sink.Entries[0].Error.Should().Be("bad");
    }

    [Fact]
    public async Task Does_not_audit_queries()
    {
        var sink = new FakeAuditSink();
        var behavior = new AuditBehavior<FakeQuery, Result<int>>(sink, new FakeUser(), NullLogger<AuditBehavior<FakeQuery, Result<int>>>.Instance);

        await behavior.Handle(new FakeQuery(), () => Task.FromResult(Result.Success(1)), default);

        sink.Entries.Should().BeEmpty();
    }

    private sealed class FakeAuditSink : IAuditSink
    {
        public List<AuditEntry> Entries { get; } = new();
        public Task WriteAsync(AuditEntry entry, CancellationToken ct) { Entries.Add(entry); return Task.CompletedTask; }
    }

    private sealed class FakeUser : ICurrentUser
    {
        public bool IsAuthenticated => true;
        public string? UserId => "u1";
        public string? UserName => "tester";
        public bool IsSuperAdmin => false;
        public IReadOnlySet<string> Permissions => new HashSet<string>();
        public IReadOnlyCollection<string> DataScopePaths => [];
        public IReadOnlyCollection<Guid> DataScopeUnitIds => [];
        public bool HasPermission(string permission) => false;
    }
}
