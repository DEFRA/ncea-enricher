using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ncea.enricher.Processor.Contracts;

namespace Ncea.Enricher.Tests.Clients;

public static class OrchestrationServiceForTests
{
    public static void Get<T>(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IAzureClientFactory<ShareClient>> mockFileShareClientFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<T>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient)
    {
        List<KeyValuePair<string, string?>> lstProps = new List<KeyValuePair<string, string?>>();
        lstProps.Add(new KeyValuePair<string, string?>("HarvesterQueueName", "test-HarvesterQueueName"));
        lstProps.Add(new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"));

        configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();
        mockServiceBusProcessorFactory = new Mock<IAzureClientFactory<ServiceBusProcessor>>();
        mockFileShareClientFactory = new Mock<IAzureClientFactory<ShareClient>>();


        mockServiceBusProcessor = new Mock<ServiceBusProcessor>();
        mockFileShareServiceClient = new Mock<ShareServiceClient> ();
        mockShareClient = new Mock<ShareClient>();
        mockShareDirectoryClient = new Mock<ShareDirectoryClient>();
        mockShareFileClient = new Mock<ShareFileClient>();

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
        mockShareDirectoryClient.Setup(x => x.GetFileClient(It.IsAny<string>())).Returns(mockShareFileClient.Object);
        mockShareClient.Setup(x => x.GetDirectoryClient(It.IsAny<string>())).Returns(mockShareDirectoryClient.Object);
        mockShareClient.Setup(x => x.CreateIfNotExistsAsync(It.IsAny<ShareCreateOptions>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(It.IsAny<Azure.Response<ShareInfo>>()));
        mockFileShareServiceClient.Setup(x => x.GetShareClient(It.IsAny<string>())).Returns(mockShareClient.Object);
    }
}
