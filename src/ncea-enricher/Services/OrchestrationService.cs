using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class OrchestrationService : IOrchestrationService
{
    private const string ProcessorErrorMessage = "Error in processing message in ncea-enricher service";
    private const string SaveFileErrorMessage = "Error occured while saving the enriched metadata file";
    private readonly string _fileShareName;
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrchestrationService> _logger;
    private string? _fileIdentifier;

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
            _fileIdentifier = GetFileIdentifier(body)!;

            var dataSource = args.Message.ApplicationProperties["DataSource"].ToString();
            var mdcMappedData = await _serviceProvider.GetRequiredKeyedService<IEnricherService>(dataSource).Enrich(_fileIdentifier, body);

            await SaveEnrichedXmlAsync(mdcMappedData, dataSource!);

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
        
    private async Task SaveEnrichedXmlAsync(string mdcMappedData, string dataSource)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(mdcMappedData))
            {
                throw new ArgumentException("Enriched xml content should not be null or empty");
            }
            _fileIdentifier = _fileIdentifier ?? GetFileIdentifier(mdcMappedData);
            if (string.IsNullOrWhiteSpace(_fileIdentifier))
            {
                throw new ArgumentException("Missing FileIdentifier in Enriched xml content");
            }

            var fileName = string.Concat(_fileIdentifier, ".xml");
            var filePath = Path.Combine(_fileShareName, dataSource, fileName);

            using (var uploadStream = GenerateStreamFromString(mdcMappedData))
            {
                using (var fileStream = File.Create(filePath))
                {
                    await uploadStream.CopyToAsync(fileStream);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, SaveFileErrorMessage);
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
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlString);
        var xDoc = XDocument.Load(xmlDoc!.CreateNavigator()!.ReadSubtree());
        var rootNode = xDoc.Root;

        var reader = xDoc.CreateReader();
        var nsMgr = new XmlNamespaceManager(reader.NameTable);
        nsMgr.AddNamespace("gmd", "http://www.isotc211.org/2005/gmd");
        nsMgr.AddNamespace("gco", "http://www.isotc211.org/2005/gco");
        nsMgr.AddNamespace("gmx", "http://www.isotc211.org/2005/gmx");

        var identifierNode = rootNode!.XPathSelectElement("//gmd:fileIdentifier/gco:CharacterString", nsMgr);
        return identifierNode != null ? identifierNode.Value : null;
    }
}
