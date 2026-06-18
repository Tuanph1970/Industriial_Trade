using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.Reporting.Application.Campaigns;
using IndustryTrade.Modules.Reporting.Application.Submissions;
using IndustryTrade.Modules.Reporting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Reporting.Infrastructure;

public static class ReportingInfrastructure
{
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<ReportingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", ReportingDbContext.Schema));
            options.AddInterceptors(new OutboxWriterInterceptor());
        });
        services.AddOutboxProcessor<ReportingDbContext>();

        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        return services;
    }
}
