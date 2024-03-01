using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processors;

public class JnccEnricher : IEnricherService
{
    private readonly ILogger<JnccEnricher> _logger;

    public JnccEnricher(ILogger<JnccEnricher> logger)
    {
        _logger = logger;
    }
    public async Task<string> Enrich(string mappedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Jncc enricher");
        return await Task.FromResult(mappedData);
    }
}
