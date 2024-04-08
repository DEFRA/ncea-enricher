﻿using Ncea.Enricher.Processors.Contracts;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ISynonymsProvider synonymsProvider, ILogger<MedinEnricher> logger)
    {
        _synonymsProvider = synonymsProvider;
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
