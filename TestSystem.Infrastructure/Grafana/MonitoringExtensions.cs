using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace TestSystem.Infrastructure.Grafana;

public static class MonitoringExtensions
{
    public static IServiceCollection AddGlobalMetrics(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation() 
                    .AddPrometheusExporter();
            });

        return services;
    }
    public static void UseGlobalMetrics(this IApplicationBuilder app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
    }
}