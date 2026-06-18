using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IndustryTrade.Api;

internal static class Observability
{
    /// <summary>
    /// OpenTelemetry traces + metrics. Traces (ASP.NET Core + Npgsql) export via OTLP only when
    /// <c>OpenTelemetry:OtlpEndpoint</c> is configured (so there are no exporter errors without a
    /// collector). Metrics (ASP.NET Core + .NET runtime) are exposed at <c>/metrics</c> for Prometheus.
    /// </summary>
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        const string serviceName = "industry-trade-api";
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddNpgsql();
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddRuntimeInstrumentation();
                metrics.AddPrometheusExporter();
            });

        return builder;
    }
}
