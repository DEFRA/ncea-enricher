using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ncea.enricher.Processor;
using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Processors;
using Ncea.Enricher.Tests.Clients;
using ncea_enricher.tests.Clients;
using System.Reflection;

namespace Ncea.Enricher.Tests.Processors;

public class OrchestrationServiceTests
{
    [Fact]
    public async Task StartProcessorAsync_ShouldStartProcessorAsyncOnServiceBusProcessor()
    {
        // Arrange
        OrchestrationServiceForTests.Get<OrchestrationService>(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var serviceProvider = new Mock<IServiceProvider>();
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, serviceProvider.Object, loggerMock.Object, mockShareClient.Object);

        // Act
        await service.StartProcessorAsync(It.IsAny<CancellationToken>());

        // Assert
        mockServiceBusProcessor.Verify(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProcessor_ShouldCall_CreateProcessor_On_ServiceBusClient()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        await service.StartProcessorAsync(It.IsAny<CancellationToken>());

        // Assert        
        mockServiceBusProcessor.Verify(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ErrorHandlerAsync_Should_Complete_Task()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();

        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var args = new ProcessErrorEventArgs(new Exception("test-exception"), It.IsAny<ServiceBusErrorSource>(),
                                             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                             It.IsAny<CancellationToken>());
        var errorHandlerMethod = typeof(OrchestrationService).GetMethod("ErrorHandlerAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)errorHandlerMethod?.Invoke(service, new object[] { args });


        // Act        
        if (task != null) await task;


        // Assert
        loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        Assert.True(task?.IsCompleted);
    }

    [Fact]
    public async Task ProcessMessagesAsync_Should_CompleteMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor, 
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        LoggerForTests.Get<MedinEnricher>(out Mock<ILogger<MedinEnricher>> mockLogger);

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";
        var serviceBusMessageProps = new Dictionary<string, object>();
        serviceBusMessageProps.Add("DataSource", "Medin");
        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(message), messageId: "messageId", properties: serviceBusMessageProps);
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockServiceProvider = ServiceProviderForTests.Get();


        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider, loggerMock.Object, mockShareClient.Object);
        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));
        if (task != null) await task;

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WithError_Should_AbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor, 
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: null, messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var mockServiceProvider = new Mock<IServiceProvider>();
        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));
        if (task != null) await task;

        // Assert
        loggerMock.Verify(x => x.Log(LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadToFileShareAsync_Should_Not_UploadFile()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor, 
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        
        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("UploadToFileShareAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { string.Empty, It.IsAny<string>() }));
        if (task != null) await task;

        // Assert
        mockFileShareServiceClient.Verify(x => x.GetShareClient(It.IsAny<string>()), Times.Never);
    }

    //[Fact]
    //public async Task UploadToFileShareAsync_Should_UploadFile()
    //{
    //    // Arrange
    //    OrchestrationServiceForTests.Get(out IConfiguration configuration,
    //                        out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
    //                        out Mock<IOrchestrationService> mockOrchestrationService,
    //                        out Mock<ILogger<OrchestrationService>> loggerMock,
    //                        out Mock<ServiceBusProcessor> mockServiceBusProcessor,
    //                        out Mock<ShareServiceClient> mockFileShareServiceClient,
    //                        out Mock<ShareClient> mockShareClient,
    //                        out Mock<ShareDirectoryClient> mockShareDirectoryClient,
    //                        out Mock<ShareFileClient> mockShareFileClient);
    //    var mockServiceProvider = new Mock<IServiceProvider>();
    //    var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
    //                    "<gmd:MD_Metadata " +
    //                    "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
    //                    "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
    //                    "<gmd:fileIdentifier>" +
    //                    "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
    //                    "</gmd:fileIdentifier>" +
    //                    "</gmd:MD_Metadata>";

    //    // Act
    //    var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
    //    var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("UploadToFileShareAsync", BindingFlags.NonPublic | BindingFlags.Instance);
    //    var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { message, It.IsAny<string>() }));
    //    if (task != null) await task;

    //    // Assert
    //    mockFileShareServiceClient.Verify(x => x.GetShareClient(It.IsAny<string>()), Times.Once);
    //    mockShareClient.Verify(x => x.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
    //    mockShareClient.Verify(x => x.GetDirectoryClient(It.IsAny<string>()), Times.Once);
    //    mockShareDirectoryClient.Verify(x => x.GetFileClient(It.IsAny<string>()), Times.Once);
    //    mockFileShareServiceClient.Verify(x => x.GetShareClient(It.IsAny<string>()), Times.Once);
    //}

    [Fact]
    public void GetFileIdentifier_Should_Return_FileIdentifier()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>test-file-identifier</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var GetFileIdentifierMethod = typeof(OrchestrationService).GetMethod("GetFileIdentifier", BindingFlags.NonPublic | BindingFlags.Static);
        var fileIdentifier = (string?)(GetFileIdentifierMethod?.Invoke(service, new object[] { message }));


        // Assert
        Assert.Equal("test-file-identifier", fileIdentifier!);
    }

    [Fact]
    public void GetFileIdentifier_Should_Return_null()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var GetFileIdentifierMethod = typeof(OrchestrationService).GetMethod("GetFileIdentifier", BindingFlags.NonPublic | BindingFlags.Static);
        var fileIdentifier = (string?)(GetFileIdentifierMethod?.Invoke(service, new object[] { message }));


        // Assert
        Assert.Null(fileIdentifier!);
    }

    [Fact]
    public void GenerateStreamFromString_Should_Return_Stream()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>test-file-identifier</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var GenerateStreamFromStringMethod = typeof(OrchestrationService).GetMethod("GenerateStreamFromString", BindingFlags.NonPublic | BindingFlags.Static);
        var fileStream = (Stream?)(GenerateStreamFromStringMethod?.Invoke(service, new object[] { message }));


        // Assert
        Assert.NotNull(fileStream);
    }

    [Fact]
    public void GenerateStreamFromString_Should_Not_Return_Stream()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor,
                            out Mock<ShareServiceClient> mockFileShareServiceClient,
                            out Mock<ShareClient> mockShareClient,
                            out Mock<ShareDirectoryClient> mockShareDirectoryClient,
                            out Mock<ShareFileClient> mockShareFileClient);
        var mockServiceProvider = new Mock<IServiceProvider>();
        var message = "";

        // Act
        var service = new OrchestrationService(configuration, mockServiceBusProcessorFactory.Object, mockServiceProvider.Object, loggerMock.Object, mockShareClient.Object);
        var GenerateStreamFromStringMethod = typeof(OrchestrationService).GetMethod("GenerateStreamFromString", BindingFlags.NonPublic | BindingFlags.Static);
        var fileStream = (Stream?)(GenerateStreamFromStringMethod?.Invoke(service, new object[] { message }));


        // Assert
        Assert.Equal(0, fileStream!.Length);
    }
}
