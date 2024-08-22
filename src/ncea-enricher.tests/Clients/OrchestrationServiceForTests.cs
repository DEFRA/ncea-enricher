using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Tests.Clients;

public static class OrchestrationServiceForTests
{
    public static void Get<T>(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<T>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor)
    {
        var dirPath = Directory.GetCurrentDirectory();
        Directory.CreateDirectory(Path.Combine(dirPath, @"medin"));
        Directory.CreateDirectory(Path.Combine(dirPath, @"medin-new"));
        List<KeyValuePair<string, string?>> lstProps =
        [
            new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
            new KeyValuePair<string, string?>("FileShareName", dirPath),
        ];

        configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();
        mockServiceBusProcessorFactory = new Mock<IAzureClientFactory<ServiceBusProcessor>>();

        mockServiceBusProcessor = new Mock<ServiceBusProcessor>();

        loggerMock = new Mock<ILogger<T>>(MockBehavior.Strict);
        loggerMock.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );
        loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );
        mockOrchestrationService = new Mock<IOrchestrationService>();

        // Set up the mock to return the mock sender        
        mockServiceBusProcessor.Setup(x => x.StartProcessingAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockOrchestrationService.Setup(x => x.StartProcessorAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockServiceBusProcessor.Setup(x => x.StopProcessingAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockServiceBusProcessorFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockServiceBusProcessor.Object);        
    }
}
