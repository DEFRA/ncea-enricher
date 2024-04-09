using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Processors.Contracts;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly IXmlSearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ISynonymsProvider synonymsProvider, 
        IXmlSearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        ILogger<MedinEnricher> logger)
    {
        _synonymsProvider = synonymsProvider;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _logger = logger;
    }
    public async Task<string> Enrich(string mappedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Medin enricher");

        var responseXml = XDocument.Parse(mappedData);
        var rootNode = responseXml.Root;
        var classifiers = await _synonymsProvider.GetAll(cancellationToken);

        if (rootNode != null)
        {

        }        

        return await Task.FromResult(responseXml.ToString());
    }
}
