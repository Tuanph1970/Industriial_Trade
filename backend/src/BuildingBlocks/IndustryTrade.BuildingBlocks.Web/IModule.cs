using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IndustryTrade.BuildingBlocks.Web;

/// <summary>
/// Contract every bounded context implements so the host can compose modules uniformly.
/// This is the seam that keeps the modular monolith "extractable": today modules register
/// in-process; tomorrow a module can move behind HTTP/gRPC without the host changing shape.
/// </summary>
public interface IModule
{
    string Name { get; }

    /// <summary>Register the module's services (handlers, repositories, DbContext, validators).</summary>
    IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Map the module's HTTP endpoints.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>Apply the module's database migrations. Default: no-op (override per module).</summary>
    Task ApplyMigrationsAsync(IServiceProvider services) => Task.CompletedTask;
}
