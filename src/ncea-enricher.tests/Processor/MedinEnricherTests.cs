using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Xml;

namespace Ncea.Enricher.Tests.Processor;

public class MedinEnricherTests
{
    private IServiceProvider _serviceProvider;
    private IBlobStorageService _blobStorageService;
    private ISynonymsProvider _synonymsProvider;
    private ISearchableFieldConfigurations _searchableFieldConfigurations;
    private ISearchService _searchService;
    private IXmlNodeService _nodeService;
    private ILogger<MedinEnricher> _logger;

    public MedinEnricherTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        var configuration = _serviceProvider.GetService<IConfiguration>()!;
        _blobStorageService = BlobServiceForTests.Get();
        _synonymsProvider = new SynonymsProvider(configuration, _blobStorageService);
        _searchableFieldConfigurations = new SearchableFieldConfigurations(configuration);
        _searchService = new SearchService();
        _nodeService = new XmlNodeService(configuration);
        _logger = _serviceProvider.GetService<ILogger<MedinEnricher>>()!;
    }
    [Fact]
    public async Task Enrich_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(filePath);

        var medinService = new MedinEnricher(_synonymsProvider, _searchableFieldConfigurations, _searchService, _nodeService, _logger);
        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await medinService.Enrich("test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }
}