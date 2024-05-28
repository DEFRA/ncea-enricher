using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Text;
using System.Xml;

namespace Ncea.Enricher.Tests.Processor;

public class MdcEnricherTests
{
    private IServiceProvider _serviceProvider;
    private IBlobService _blobStorageService;
    private ISynonymsProvider _synonymsProvider;
    private ISearchableFieldConfigurations _searchableFieldConfigurations;
    private ISearchService _searchService;
    private IXmlNodeService _nodeService;
    private IXmlValidationService _xmlValidationService;
    private IConfiguration _configuration;
    private ILogger<MdcEnricher> _logger;

    public MdcEnricherTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _configuration = _serviceProvider.GetService<IConfiguration>()!;
        _blobStorageService = BlobServiceForTests.Get();
        _synonymsProvider = new SynonymsProvider(_configuration, _blobStorageService);
        _searchableFieldConfigurations = new SearchableFieldConfigurations(_configuration);
        _searchService = new SearchService();
        _nodeService = new XmlNodeService(_configuration);

        LoggerForTests.Get(out Mock<ILogger<XPathValidationService>> loggerMock);
        _xmlValidationService = new XPathValidationService(_configuration, loggerMock.Object);
        _logger = _serviceProvider.GetService<ILogger<MdcEnricher>>()!;
    }

    [Fact]
    public async Task Enrich_WhenFeatureFlagEnabled_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MetadataEnrichmentFeature)).ReturnsAsync(true);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MdcValidationFeature)).ReturnsAsync(true);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "fff8010e6a805ba79102d35dbdda4d93.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(new StreamReader(filePath, Encoding.UTF8));

        var medinService = new MdcEnricher(_synonymsProvider, _searchableFieldConfigurations, _searchService, _nodeService, _xmlValidationService, featureManagerMock.Object);
        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await medinService.Enrich(It.IsAny<string>(), "test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task Enrich_WhenFeatureFlagDisabled_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MetadataEnrichmentFeature)).ReturnsAsync(false);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MdcValidationFeature)).ReturnsAsync(false);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "fff8010e6a805ba79102d35dbdda4d93.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(new StreamReader(filePath, Encoding.UTF8));

        var medinService = new MdcEnricher(_synonymsProvider, _searchableFieldConfigurations, _searchService, _nodeService, _xmlValidationService, featureManagerMock.Object);
        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await medinService.Enrich(It.IsAny<string>(), "test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }
}