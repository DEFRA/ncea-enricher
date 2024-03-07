using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher;

[ExcludeFromCodeCoverage]
public class HealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));
    }
}