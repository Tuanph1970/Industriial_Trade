using FluentValidation;
using IndustryTrade.BuildingBlocks.Web;
using IndustryTrade.Modules.Files.Api.Endpoints;
using IndustryTrade.Modules.Files.Application.Files;
using IndustryTrade.Modules.Files.Infrastructure;
using IndustryTrade.Modules.Files.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.Modules.Files.Api;

public sealed class FilesModule : IModule
{
    public string Name => "Files";

    public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFilesInfrastructure(configuration);

        var applicationAssembly = typeof(UploadFileCommand).Assembly;
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapFileEndpoints();

    public async Task ApplyMigrationsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FilesDbContext>();
        await db.Database.MigrateAsync();
    }
}
