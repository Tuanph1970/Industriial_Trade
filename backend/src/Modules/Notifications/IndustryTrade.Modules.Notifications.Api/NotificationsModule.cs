using IndustryTrade.BuildingBlocks.Application.Paging;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Notifications.Application;
using IndustryTrade.Modules.Notifications.Infrastructure;
using IndustryTrade.Modules.Notifications.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Notifications.Api;

public sealed class NotificationsModule : IModule
{
    public string Name => "Notifications";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationsInfrastructure(configuration);
        // Registers the query/command handlers AND the ReportStateChanged notification handler.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetNotificationsQuery).Assembly));
        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (ISender sender, int page = 1, int pageSize = 20, bool unreadOnly = false) =>
            ApiResults.Match(await sender.Send(new GetNotificationsQuery(new PageRequest(page, pageSize), unreadOnly))));

        group.MapGet("/unread-count", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new GetUnreadCountQuery())));

        group.MapPost("/{id:guid}/read", async (ISender sender, Guid id) =>
            ApiResults.Match(await sender.Send(new MarkNotificationReadCommand(id))));

        group.MapPost("/read-all", async (ISender sender) =>
            ApiResults.Match(await sender.Send(new MarkAllNotificationsReadCommand())));
    }

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await db.Database.MigrateAsync();
    }
}
