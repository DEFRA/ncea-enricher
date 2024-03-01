using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processors;

public class JnccEnricher : IEnricherService
{
    private readonly ILogger<JnccEnricher> _logger;

    public JnccEnricher(ILogger<JnccEnricher> logger)
    {
        _logger = logger;
    }
    public async Task<string> Transform(string harvestedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Jncc transformer");
        return await Task.FromResult(harvestedData);
    }
}
