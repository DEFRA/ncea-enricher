using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{    
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ILogger<MedinEnricher> logger)
    {     
        _logger = logger;
    }
    public async Task<string> Transform(string harvestedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Medin transformer");
        return await Task.FromResult(harvestedData);
    }
}
