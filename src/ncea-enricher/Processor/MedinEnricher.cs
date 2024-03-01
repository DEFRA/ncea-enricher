using Ncea.Enricher.Processors.Contracts;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{    
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ILogger<MedinEnricher> logger)
    {     
        _logger = logger;
    }
    public async Task<string> Enrich(string mappeddata, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Medin enricher");
        return await Task.FromResult(mappeddata);
    }
}
