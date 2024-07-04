using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Text;
using System.Xml;

namespace Ncea.Enricher.Tests.Processor;

public class MLBasedEnricherTests
{
    private IServiceProvider _serviceProvider;
    private IMdcFieldConfigurationService _searchableFieldConfigurations;
    private IXmlNodeService _nodeService;
    private IXmlValidationService _xmlValidationService;
    private IConfiguration _configuration;
    private ILogger<SynonymBasedEnricher> _logger;
    private IClassifierPredictionService _classifierPredictionService;
    private IClassifierVocabularyProvider _classifierVocabularyProvider;

    public MLBasedEnricherTests()
    {
        _serviceProvider = ServiceProviderForTests.Get();
        _configuration = _serviceProvider.GetService<IConfiguration>()!;
        _searchableFieldConfigurations = new MdcFieldConfigurationService(_configuration);
        _nodeService = new XmlNodeService(_configuration);

        LoggerForTests.Get(out Mock<ILogger<XPathValidationService>> loggerMock);
        _xmlValidationService = new XPathValidationService(_configuration, loggerMock.Object);
        _logger = _serviceProvider.GetService<ILogger<SynonymBasedEnricher>>()!;

        _classifierPredictionService = _serviceProvider.GetService<IClassifierPredictionService>()!;
        _classifierVocabularyProvider = _serviceProvider.GetService<IClassifierVocabularyProvider>()!;
    }

    [Fact]
    public async Task Enrich_WhenMLFeatureFlagEnabled_ReturnEnrichedMetadataXmlWithNceaClassifiers()
    {
        //Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.SynonymBasedClassificationFeature)).ReturnsAsync(false);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MdcValidationFeature)).ReturnsAsync(true);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature)).ReturnsAsync(true);

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = new XmlDocument();
        xDoc.Load(new StreamReader(filePath, Encoding.UTF8));

        var enricherService = new MLBasedEnricher(featureManagerMock.Object,
            _nodeService,
            _xmlValidationService,
            _searchableFieldConfigurations,
            _classifierPredictionService,
            _classifierVocabularyProvider);

        var mappedMetadataXml = xDoc.OuterXml;

        // Act
        var result = await enricherService.Enrich(It.IsAny<string>(), "test-file-id", mappedMetadataXml, It.IsAny<CancellationToken>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
    }
}