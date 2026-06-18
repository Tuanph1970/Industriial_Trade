using IndustryTrade.BuildingBlocks.Infrastructure.Outbox;
using IndustryTrade.Modules.IdentityAccess.Application.Organizations;
using IndustryTrade.Modules.IdentityAccess.Application.Roles;
using IndustryTrade.Modules.IdentityAccess.Application.Users;
using IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure;

public static class IdentityAccessInfrastructure
{
    public static IServiceCollection AddIdentityAccessInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres.");

        services.AddDbContext<IdentityAccessDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentityAccessDbContext.Schema);
                npgsql.UseNetTopologySuite();
            });
            options.AddInterceptors(new OutboxWriterInterceptor());
        });
        services.AddOutboxProcessor<IdentityAccessDbContext>();

        services.AddScoped<IOrgUnitRepository, OrgUnitRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAuthorizationProvider, UserAuthorizationProvider>();
        return services;
    }
}
