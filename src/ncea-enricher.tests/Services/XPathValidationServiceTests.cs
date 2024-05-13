using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Xml.Linq;

namespace Ncea.Enricher.Tests.Services;

public class XPathValidationServiceTests
{
    private readonly IXmlValidationService _xpathValidationService;
    private readonly Mock<ILogger<XPathValidationService>> _loggerMock;
    
    public XPathValidationServiceTests()
    {
        LoggerForTests.Get(out _loggerMock);
        var serviceProvider = ServiceProviderForTests.Get();
        var configuration = serviceProvider!.GetService<IConfiguration>()!;
        _xpathValidationService = new XPathValidationService(configuration, _loggerMock.Object);
    }

    [Fact]
    public void Validate_WhenTheGivenXmlIsValid_ThenNoExceptionThrown()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xpathValidationService.Validate(xDoc!);

        //Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(0));
    }
}
