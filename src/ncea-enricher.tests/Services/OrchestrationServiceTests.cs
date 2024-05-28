using Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ncea.Enricher.BusinessExceptions;
using Ncea.Enricher.Infrastructure;
using Ncea.Enricher.Infrastructure.Contracts;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Tests.Clients;
using System.Reflection;
using System.Xml.Schema;

namespace Ncea.Enricher.Tests.Services;

public class OrchestrationServiceTests
{
    private Mock<IEnricherService> _enricherServiceMock;
    public OrchestrationServiceTests()
    {
        _enricherServiceMock = new Mock<IEnricherService>();
    }

    [Fact]
    public async Task StartProcessorAsync_WhenReceivingTheMessage_ThenStartProcessingTheMessage()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobService = BlobServiceForTests.GetMdcXml();

        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        // Act
        await service.StartProcessorAsync(It.IsAny<CancellationToken>());

        // Assert
        mockServiceBusProcessor.Verify(x => x.StartProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ErrorHandlerAsync_WhenMessageProcessingFailed_ThenCallErrorHandler()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

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
    public async Task ProcessMessagesAsync_WhenEnrichedMetadataContentIsNotEmpty_ThenCompleteThaTaskSucessfully()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var blobServiceMock = new Mock<IBlobService>();
        blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContent);
        
        var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";
        
        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var service = new OrchestrationService(configuration,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<EnricherArgumentException>(() => task!);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WhenEnrichedMetadataContentIsEmpty_ThenThrowExceptionAndAbandonMessage()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var blobServiceMock = new Mock<IBlobService>();
        blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContent);

        var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";
        
        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageBody);

        // Act
        var service = new OrchestrationService(configuration,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        if (task != null) await task;

        // Assert
        Assert.True(task?.IsCompleted);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WhenMessageBodyIsEmpty_ThenThrowExceptionAndAbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);        

        var blobServiceMock = new Mock<IBlobService>();

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: null, messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var service = new OrchestrationService(configuration,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<EnricherArgumentException>(() => task!);
    }

    //[Fact]
    //public async Task ProcessMessagesAsync_WhenFileShareNotExists_ThenThrowExceptionAndAbandonMessageAsync()
    //{
    //    // Arrange
    //    OrchestrationServiceForTests.Get(out IConfiguration configuration,
    //                        out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
    //                        out Mock<IOrchestrationService> mockOrchestrationService,
    //                        out Mock<ILogger<OrchestrationService>> loggerMock,
    //                        out Mock<ServiceBusProcessor> mockServiceBusProcessor);

    //    var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
    //                    "<gmd:MD_Metadata " +
    //                    "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
    //                    "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
    //                    "<gmd:fileIdentifier>" +
    //                    "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
    //                    "</gmd:fileIdentifier>" +
    //                    "</gmd:MD_Metadata>";

    //    var blobServiceMock = new Mock<IBlobService>();
    //    blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(blobContent);

    //    List<KeyValuePair<string, string?>> lstProps =
    //    [
    //        new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
    //        new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
    //    ];

    //    var config = new ConfigurationBuilder()
    //                        .AddInMemoryCollection(lstProps)
    //                        .Build();

    //    var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";

    //    var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
    //    var mockReceiver = new Mock<ServiceBusReceiver>();
    //    var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
    //    var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
    //    mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    //    mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    //    _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(blobContent);
        
    //    // Act
    //    var service = new OrchestrationService(config,
    //        blobServiceMock.Object,
    //        mockServiceBusProcessorFactory.Object,
    //        _enricherServiceMock.Object,
    //        loggerMock.Object);

    //    var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
    //    var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

    //    // Assert
    //    mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    //    await Assert.ThrowsAsync<EnricherArgumentException>(() => task!);
    //}

    [Fact]
    public async Task ProcessMessagesAsync_WhenSynonymFileNotAccessible_ThenThrowExceptionAndAbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var blobServiceMock = new Mock<IBlobService>();
        blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContent);

        var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";

        List<KeyValuePair<string, string?>> lstProps =
        [
            new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
            new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
        ];

        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "Status: 404 (The specified blob does not exist.)"));

        // Act
        var service = new OrchestrationService(config,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<BlobStorageNotAccessibleException>(() => task!);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WhenUnexpectedExceptionOccurs_ThenThrowExceptionAndAbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        List<KeyValuePair<string, string?>> lstProps =
        [
            new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
            new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
        ];

        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();

        var serviceBusMessageProps = new Dictionary<string, object>
        {
            { "DataSource", "Medin" }
        };

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(message), messageId: "messageId", properties: serviceBusMessageProps);
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("test-error-message"));

        // Act
        var service = new OrchestrationService(config,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<EnricherException>(() => task!);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WhenMdcValidationFails_ThenThrowExceptionAndAbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var blobServiceMock = new Mock<IBlobService>();
        blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContent);

        var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";


        List<KeyValuePair<string, string?>> lstProps =
        [
            new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
            new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
        ];

        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new XmlSchemaValidationException("test-error-message"));

        // Act
        var service = new OrchestrationService(config,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<XmlValidationException>(() => task!);
    }

    [Fact]
    public async Task ProcessMessagesAsync_WhenTheFileShareNotExists_ThenThrowExceptionAndAbandonMessageAsync()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);

        var blobContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        var blobServiceMock = new Mock<IBlobService>();
        blobServiceMock.Setup(x => x.GetContentAsync(It.IsAny<GetBlobContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(blobContent);

        var messageBody = "{ \"FileIdentifier\":\"\",\"DataSource\":\"Medin\"}";

        List<KeyValuePair<string, string?>> lstProps =
        [
            new KeyValuePair<string, string?>("EnricherQueueName", "test-EnricherQueueName"),
            new KeyValuePair<string, string?>("FileShareName", Directory.GetCurrentDirectory()),
        ];

        var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(lstProps)
                            .Build();        

        var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: new BinaryData(messageBody), messageId: "messageId");
        var mockReceiver = new Mock<ServiceBusReceiver>();
        var processMessageEventArgs = new ProcessMessageEventArgs(receivedMessage, It.IsAny<ServiceBusReceiver>(), It.IsAny<CancellationToken>());
        var mockProcessMessageEventArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, new object[] { receivedMessage, mockReceiver.Object, It.IsAny<string>(), It.IsAny<CancellationToken>() });
        mockProcessMessageEventArgs.Setup(receiver => receiver.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockProcessMessageEventArgs.Setup(receiver => receiver.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _enricherServiceMock.Setup(x => x.Enrich(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DirectoryNotFoundException("test-error-message"));

        // Act
        var service = new OrchestrationService(config,
            blobServiceMock.Object,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("ProcessMessagesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { mockProcessMessageEventArgs.Object }));

        // Assert
        mockProcessMessageEventArgs.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        await Assert.ThrowsAsync<FileShareNotFoundException>(() => task!);
    }

    [Fact]
    public async Task SaveEnrichedXmlAsync_WhenMdcMappedMetadataXmlContentIsEmpty_ThenExceptionShouldBeThrown()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("SaveEnrichedXmlAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { string.Empty, It.IsAny<string>() }));

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => task!);
    }

    [Fact]
    public async Task SaveEnrichedXmlAsync_WhenMdcMappedMetadataXmlContentIsNotEmpty_ThenTaskCompletesWithSucess()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>Marine_Scotland_FishDAC_1740</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var processMessagesAsyncMethod = typeof(OrchestrationService).GetMethod("SaveEnrichedXmlAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task?)(processMessagesAsyncMethod?.Invoke(service, new object[] { message, "medin" }));
        if (task != null) await task;

        // Assert
        Assert.True(task?.IsCompleted);
    }

    [Fact]
    public void GetFileIdentifier_WhenFileIdentifierExistsInEnrichedContent_ReturnFileIdentifier()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>test-file-identifier</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var GetFileIdentifierMethod = typeof(OrchestrationService).GetMethod("GetFileIdentifier", BindingFlags.NonPublic | BindingFlags.Static);
        var fileIdentifier = (string?)(GetFileIdentifierMethod?.Invoke(service, new object[] { message }));

        // Assert
        Assert.Equal("test-file-identifier", fileIdentifier!);
    }

    [Fact]
    public void GetFileIdentifier_WhenFileIdentifierNotExistsInEnrichedContent_ReturnNull()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);

        var GetFileIdentifierMethod = typeof(OrchestrationService).GetMethod("GetFileIdentifier", BindingFlags.NonPublic | BindingFlags.Static);
        var fileIdentifier = (string?)(GetFileIdentifierMethod?.Invoke(service, new object[] { message }));

        // Assert
        Assert.Null(fileIdentifier!);
    }

    [Fact]
    public void GenerateStreamFromString_WhenEnrichedContentIsValidXml_ThenReturnStream()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> " +
                        "<gmd:MD_Metadata " +
                        "xmlns:gmd=\"http://www.isotc211.org/2005/gmd\" " +
                        "xmlns:gco=\"http://www.isotc211.org/2005/gco\"> " +
                        "<gmd:fileIdentifier>" +
                        "<gco:CharacterString>test-file-identifier</gco:CharacterString>" +
                        "</gmd:fileIdentifier>" +
                        "</gmd:MD_Metadata>";

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);
        var GenerateStreamFromStringMethod = typeof(OrchestrationService).GetMethod("GenerateStreamFromString", BindingFlags.NonPublic | BindingFlags.Static);
        var fileStream = (Stream?)(GenerateStreamFromStringMethod?.Invoke(service, new object[] { message }));

        // Assert
        Assert.NotNull(fileStream);
    }

    [Fact]
    public void GenerateStreamFromString_WhenEnrichedContentIsNotValidXml_ThenEmptyStream()
    {
        // Arrange
        OrchestrationServiceForTests.Get(out IConfiguration configuration,
                            out Mock<IAzureClientFactory<ServiceBusProcessor>> mockServiceBusProcessorFactory,
                            out Mock<IOrchestrationService> mockOrchestrationService,
                            out Mock<ILogger<OrchestrationService>> loggerMock,
                            out Mock<ServiceBusProcessor> mockServiceBusProcessor);
        var blobService = BlobServiceForTests.GetMdcXml();

        var message = "";

        // Act
        var service = new OrchestrationService(configuration,
            blobService,
            mockServiceBusProcessorFactory.Object,
            _enricherServiceMock.Object,
            loggerMock.Object);
        var GenerateStreamFromStringMethod = typeof(OrchestrationService).GetMethod("GenerateStreamFromString", BindingFlags.NonPublic | BindingFlags.Static);
        var fileStream = (Stream?)(GenerateStreamFromStringMethod?.Invoke(service, new object[] { message }));


        // Assert
        Assert.Equal(0, fileStream!.Length);
    }
}
