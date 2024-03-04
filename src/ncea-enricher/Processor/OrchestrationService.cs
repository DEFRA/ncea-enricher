using System.Xml;
using System.Xml.Linq;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Azure;
using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Processors.Contracts;

namespace ncea.enricher.Processor;

public class OrchestrationService : IOrchestrationService
{
    #region Initialization
    private readonly ShareClient _fileShareClient;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrchestrationService> _logger;

    public OrchestrationService(IConfiguration configuration,
        IAzureClientFactory<ServiceBusProcessor> serviceBusProcessorFactory,
        IAzureClientFactory<ShareClient> fileShareClientFactory,
        IServiceProvider serviceProvider,
        ILogger<OrchestrationService> logger)
    {
        var mapperQueueName = configuration.GetValue<string>("MapperQueueName");
        var fileShareName = configuration.GetValue<string>("FileShareName")!;

        _processor = serviceBusProcessorFactory.CreateClient(mapperQueueName);
        _fileShareClient = fileShareClientFactory.CreateClient(fileShareName);
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    #endregion

    #region ServiceBusMessage Processor
    public async Task StartProcessorAsync(CancellationToken cancellationToken = default)
    {
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ErrorHandlerAsync;
        await _processor.StartProcessingAsync(cancellationToken);
    }    

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        _logger.LogInformation("Received a messaage to enrich metadata");
        try
        {
            var body = args.Message.Body.ToString();
            var dataSource = args.Message.ApplicationProperties["DataSource"].ToString();            ;
            var mdcMappedData = await _serviceProvider.GetRequiredKeyedService<IEnricherService>(dataSource).Enrich(body);
            
            await UploadToFileShareAsync(mdcMappedData, dataSource!);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing message: {ex.Message}", ex.Message);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError("Error processing message: {args.Exception.Message}", args.Exception.Message);
        return Task.CompletedTask;
    }
    #endregion

    #region File share
    private async Task UploadToFileShareAsync(string message, string dataSource)
    {
        _logger.LogInformation("Upload started");

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError("Empty enriched xml.failed uploading.");
            return;
        }

        var fileIdentifier = GetFileIdentifier(message);

        if (string.IsNullOrWhiteSpace(fileIdentifier))
        {
            _logger.LogError("Invalid enriched xml.failed uploading.");
            return;
        }
        _logger.LogInformation("Upload in-progress for {fileIdentifier}", fileIdentifier);

        try
        {
            if (await _fileShareClient.ExistsAsync())
            {
                _logger.LogInformation("Upload started - 1");
                var directory = _fileShareClient.GetDirectoryClient(dataSource);
                await directory.CreateIfNotExistsAsync();

                if(await directory.ExistsAsync())
                {
                    _logger.LogInformation("Upload started - 2");
                    var fileName = string.Concat(fileIdentifier, ".xml");
                    var file = directory.GetFileClient(fileName);

                    using (var fileStream = GenerateStreamFromString(message))
                    {
                        _logger.LogInformation("Upload started - 3");
                        file.Create(fileStream.Length);
                        file.UploadRange(new HttpRange(0, fileStream.Length), fileStream);
                        _logger.LogInformation("Upload started - 4");
                    }
                }                
            }                
        }
        catch(Exception ex) {
            _logger.LogError("Error occured while uploading enriched files on fileshare {ex}", ex);
        }

        _logger.LogInformation("Upload completed for {fileIdentifier}", fileIdentifier);
    }

    private static MemoryStream GenerateStreamFromString(string fileContent)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(fileContent);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string? GetFileIdentifier(string xmlString)
    {
        //Xml string to XElement Conversion
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlString);
        var xDoc = XDocument.Load(xmlDoc!.CreateNavigator()!.ReadSubtree());
        var xmlElement = xDoc.Root;

        string gmdNameSpaceString = "http://www.isotc211.org/2005/gmd";
        var fileIdentifierXmlElement = xmlElement!.Descendants()
                                                  .FirstOrDefault(n => n.Name.Namespace.NamespaceName == gmdNameSpaceString
                                                                  && n.Name.LocalName == "fileIdentifier");
        var fileIdentifier = fileIdentifierXmlElement?.Descendants()?.FirstOrDefault()?.Value;
        return fileIdentifier;
    }
    #endregion
}
