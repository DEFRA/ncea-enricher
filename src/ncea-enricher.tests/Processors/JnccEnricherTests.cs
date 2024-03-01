using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Tests.Clients;

namespace Ncea.Enricher.Tests.Processors;

public class JnccEnricherTests
{
    [Fact]
    public async Task Process_ShouldLogMessage()
    {
        //Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<JnccEnricher>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var jnccService = new JnccEnricher(loggerMock.Object);



        // Act
        await jnccService.Enrich(It.IsAny<string>(), It.IsAny<CancellationToken>());


        // Assert
        loggerMock.Verify(x => x.Log(LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
