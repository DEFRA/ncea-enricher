using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Xml.Linq;

namespace Ncea.Enricher.Tests.Services;

public class XsdValidationServiceTests
{
    private readonly XsdValidationService _xmlValidationService;
    private readonly Mock<ILogger<XsdValidationService>> _loggerMock;
    
    public XsdValidationServiceTests()
    {
        LoggerForTests.Get(out _loggerMock);
        _xmlValidationService = new XsdValidationService(_loggerMock.Object);
    }

    [Fact(Skip = "Will be removed later")]
    public void Validate_WhenTheGivenXmlIsValid_ThenNoExceptionThrown()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "fff8010e6a805ba79102d35dbdda4d93.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xmlValidationService.Validate(xDoc!, It.IsAny<string>());

        //Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));
    }

    [Fact(Skip = "Will be removed later")]
    public void Validate_WhenTheGivenXmlWithoutFileIdentifierNode_ThenThrowAnException()
    {
        //Arrange
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "xml_without_fileidentifier_node.xml");
        var xDoc = XDocument.Load(filePath);

        //Act
        _xmlValidationService.Validate(xDoc!, It.IsAny<string>());

        //Assert
        _loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));
    }
}
