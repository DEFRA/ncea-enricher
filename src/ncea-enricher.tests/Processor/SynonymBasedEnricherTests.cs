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

public class SynonymBasedEnricherTests
{
    private IServiceProvider _serviceProvider;
    private IBlobService _blobStorageService;
    private ISynonymsProvider _synonymsProvider;
    private IMdcFieldConfigurationService _searchableFieldConfigurations;
    private ISearchService _searchService;
    private IXmlNodeService _nodeService;
    private IXmlValidationService _xmlValidationService;
    private IConfiguration _configuration;
    private ILogger<SynonymBasedEnricher> _logger;

    public SynonymBasedEnricherTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _configuration = _serviceProvider.GetService<IConfiguration>()!;
        _blobStorageService = BlobServiceForTests.Get();
        _synonymsProvider = new SynonymsProvider(_configuration, _blobStorageService);
        _searchableFieldConfigurations = new MdcFieldConfigurationService(_configuration);
        _searchService = new SearchService();
        _nodeService = new XmlNodeService(_configuration);

        LoggerForTests.Get(out Mock<ILogger<XPathValidationService>> loggerMock);
        _xmlValidationService = new XPathValidationService(_configuration, loggerMock.Object);
        _logger = _serviceProvider.GetService<ILogger<SynonymBasedEnricher>>()!;
    }

    [Fact]
    public async Task Enrich_WhenSynonymFeatureFlagEnabled_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.SynonymBasedClassificationFeature)).ReturnsAsync(true);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MdcValidationFeature)).ReturnsAsync(true);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature)).ReturnsAsync(false);

        var classifierVocabularyProviderMock = new Mock<IClassifierVocabularyProvider>();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "fff8010e6a805ba79102d35dbdda4d93.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(new StreamReader(filePath, Encoding.UTF8));

        var enricherService = new SynonymBasedEnricher(featureManagerMock.Object,
            _nodeService,
            _xmlValidationService,
            _searchableFieldConfigurations,
            _searchService,
            _synonymsProvider);

        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await enricherService.Enrich(It.IsAny<string>(), "test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task Enrich_WhenSynonymFeatureFlagDisabled_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.SynonymBasedClassificationFeature)).ReturnsAsync(false);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MdcValidationFeature)).ReturnsAsync(true);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature)).ReturnsAsync(false);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "fff8010e6a805ba79102d35dbdda4d93.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(new StreamReader(filePath, Encoding.UTF8));

        var enricherService = new SynonymBasedEnricher(featureManagerMock.Object,
             _nodeService,
            _xmlValidationService,
            _searchableFieldConfigurations,
            _searchService,
            _synonymsProvider);

        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await enricherService.Enrich(It.IsAny<string>(), "test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }
}