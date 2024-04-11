using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Ncea.Enricher.Processor.Contracts;

namespace Ncea.Enricher.Processor;

public class OrchestrationService : IOrchestrationService
{    
    private const string ProcessorErrorMessage = "Error in processing message in ncea-enricher service";
    private readonly string _fileShareName;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrchestrationService> _logger;

    public OrchestrationService(IConfiguration configuration,
        IAzureClientFactory<ServiceBusProcessor> serviceBusProcessorFactory,
        IServiceProvider serviceProvider,
        ILogger<OrchestrationService> logger)
    {
        var mapperQueueName = configuration.GetValue<string>("MapperQueueName");
        _fileShareName = configuration.GetValue<string>("FileShareName")!;

        _processor = serviceBusProcessorFactory.CreateClient(mapperQueueName);
        _serviceProvider = serviceProvider;
        _logger = logger;
    }    

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
            var dataSource = args.Message.ApplicationProperties["DataSource"].ToString();
            var mdcMappedData = await _serviceProvider.GetRequiredKeyedService<IEnricherService>(dataSource).Enrich(body);
            
            await UploadToFileShareAsync(mdcMappedData, dataSource!);
            
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ProcessorErrorMessage);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, ProcessorErrorMessage);
        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    private async Task UploadToFileShareAsync(string message, string dataSource)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError("Empty enriched xml failed uploading.");
            return;
        }

        var fileIdentifier = GetFileIdentifier(message);

        if (string.IsNullOrWhiteSpace(fileIdentifier))
        {
            _logger.LogError("Invalid enriched xml.failed uploading.");
            return;
        }

        try
        {
            var fileName = string.Concat(fileIdentifier, ".xml");
            var filePath = Path.Combine(_fileShareName, dataSource, fileName);

            using (var uploadStream = GenerateStreamFromString(message))
            {
                using (var fileStream = File.Create(filePath))
                {
                    await uploadStream.CopyToAsync(fileStream);
                }
            }
        }
        catch(Exception ex) 
        {
            _logger.LogError(ex, "Error occured while uploading enriched files on fileshare");
        }
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
}
