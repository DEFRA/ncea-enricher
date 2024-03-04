using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics.CodeAnalysis;

namespace Ncea.Enricher;

[ExcludeFromCodeCoverage]
public sealed class TcpHealthProbeService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly TcpListener _listener;
    private readonly ILogger<TcpHealthProbeService> _logger;

    public TcpHealthProbeService(
        HealthCheckService healthCheckService,
        ILogger<TcpHealthProbeService> logger,
        IConfiguration config)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger;

        // Attach TCP listener to the port in configuration
        var port = config.GetValue<int?>("HealthProbe:TcpPort") ?? 5000;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Started health check service for ncea-enricher.");
        await Task.Yield();
        _listener.Start();
        while (!stoppingToken.IsCancellationRequested)
        {
            // Gather health metrics every second.
            await UpdateHeartbeatAsync(stoppingToken);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        _listener.Stop();
    }

    private async Task UpdateHeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get health check results
            var result = await _healthCheckService.CheckHealthAsync(cancellationToken);
            var isHealthy = result.Status == HealthStatus.Healthy;

            if (!isHealthy)
            {
                _listener.Stop();
                _logger.LogInformation("The ncea-enricher service is unhealthy. Listener stopped.");
                return;
            }

            _listener.Start();
            while (_listener.Server.IsBound && _listener.Pending())
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                client.Close();
                _logger.LogInformation("Successfully processed health check request for ncea-enricher service.");
            }

            _logger.LogDebug("Heartbeat check executed for ncea-enricher service.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An error occurred while checking heartbeat for ncea-enricher service.");
        }
    }
}
