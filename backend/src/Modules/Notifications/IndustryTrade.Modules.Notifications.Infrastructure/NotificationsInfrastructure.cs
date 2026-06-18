using IndustryTrade.Modules.Notifications.Application;
using IndustryTrade.Modules.Notifications.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Notifications.Infrastructure;

public static class NotificationsInfrastructure
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", NotificationDbContext.Schema)));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        return services;
    }
}
