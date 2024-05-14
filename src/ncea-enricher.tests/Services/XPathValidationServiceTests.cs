using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Services;
using Ncea.Enricher.Tests.Clients;
using System.Xml.Linq;

namespace Ncea.Enricher.Tests.Services;

public class XPathValidationServiceTests
{
    private readonly XPathValidationService _xpathValidationService;
    private readonly Mock<ILogger<XPathValidationService>> _loggerMock;

    public XPathValidationServiceTests()
    {
        var serviceProvider = ServiceProviderForTests.Get();
        var configuration = serviceProvider!.GetService<IConfiguration>()!;

        LoggerForTests.Get(out _loggerMock);
        _xpathValidationService = new XPathValidationService(configuration, _loggerMock.Object);
    }

    [Fact]
    public void Validate_WhenTheEnrichedXmlValidationIsSuccessfull_ThenNoLogWarning()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xpathValidationService.Validate(xDoc!);

        //Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void Validate_WhenTheEnrichedXmlValidationFails_ThenLogWarning()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "xml_without_fileidentifier_node.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xpathValidationService.Validate(xDoc!);

        //Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
