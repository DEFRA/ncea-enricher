using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Ncea.Enricher.BusinessExceptions;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Utils;

namespace Ncea.Enricher.Services;

public class OrchestrationService : IOrchestrationService
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";

    private const string ProcessorErrorMessage = "Error in processing message in ncea-enricher service";

    private readonly string _fileShareName;
    private readonly ServiceBusProcessor _processor;
    private readonly IEnricherService _mdcEnricherSerivice;
    private readonly ILogger<OrchestrationService> _logger;
    private string? _fileIdentifier;

    public OrchestrationService(IConfiguration configuration,
        IAzureClientFactory<ServiceBusProcessor> serviceBusProcessorFactory,
        IEnricherService mdcEnricherSerivice,
        ILogger<OrchestrationService> logger)
    {
        var mapperQueueName = configuration.GetValue<string>("MapperQueueName");
        _fileShareName = configuration.GetValue<string>("FileShareName")!;

        _processor = serviceBusProcessorFactory.CreateClient(mapperQueueName);
        _mdcEnricherSerivice = mdcEnricherSerivice;
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

        var dataSource = string.Empty;
        var mdcMappedData = args.Message.Body.ToString();

        try
        {
            dataSource = args.Message.ApplicationProperties["DataSource"].ToString();
            var dataSourceName = Enum.Parse(typeof(DataSourceNames), dataSource!, true).ToString()!.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(mdcMappedData))
            {
                throw new ArgumentException("Mappeed-queue message body should not be empty");
            }

            _fileIdentifier = GetFileIdentifier(mdcMappedData)!;
            var enrichedMetadata = await _mdcEnricherSerivice.Enrich(_fileIdentifier, mdcMappedData);

            await SaveEnrichedXmlAsync(enrichedMetadata, dataSourceName!);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleException(args, ex, new EnricherArgumentException(ex.Message, ex));
        }
        catch (RequestFailedException ex)
        {
            var errorMessage = $"Error occured while reading the synonyms file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new SynonymsNotAccessibleException(errorMessage, ex));
        }
        catch (DirectoryNotFoundException ex)
        {
            var errorMessage = $"Error occured while saving the xml file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new FileShareNotFoundException(errorMessage, ex));
        }
        catch (XmlValidationException ex)
        {
            var errorMessage = $"Error occured while validating enriched xml file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex.InnerException!, new XmlValidationException(errorMessage, ex));
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error occured during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new EnricherException(errorMessage, ex));
        }
    }
    
    private async Task SaveEnrichedXmlAsync(string mdcMappedData, string dataSource)
    {
        if (string.IsNullOrWhiteSpace(mdcMappedData))
        {
            throw new ArgumentException("Enriched xml content should not be null or empty");
        }

        var filePath = GetEnrichedXmlFilePath(mdcMappedData, dataSource);

        using (var uploadStream = GenerateStreamFromString(mdcMappedData))
        {
            using (var fileStream = File.Create(filePath))
            {
                await uploadStream.CopyToAsync(fileStream);
            }
        }        
    }

    private string GetEnrichedXmlFilePath(string mdcMappedData, string dataSource)
    {
        _fileIdentifier = _fileIdentifier ?? GetFileIdentifier(mdcMappedData);
        if (string.IsNullOrWhiteSpace(_fileIdentifier))
        {
            throw new ArgumentException("Missing FileIdentifier in Enriched xml content");
        }

        var fileName = string.Concat(_fileIdentifier, ".xml");
        var filePath = Path.Combine(_fileShareName, dataSource, fileName);
        return filePath;
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
        nsMgr.AddNamespace("gmd", GmdNamespace);
        nsMgr.AddNamespace("gco", GcoNamespace);
        nsMgr.AddNamespace("gmx", GmxNamespace);

        var identifierNode = rootNode!.XPathSelectElement("//gmd:fileIdentifier/gco:CharacterString", nsMgr);
        return identifierNode != null ? identifierNode.Value : null;
    }

    private Task ErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, ProcessorErrorMessage);
        return Task.CompletedTask;
    }

    private async Task HandleException(ProcessMessageEventArgs args, Exception ex, BusinessException businessException)
    {
        CustomLogger.LogErrorMessage(_logger, businessException.Message, ex);
        await args.AbandonMessageAsync(args.Message);
        throw businessException;
    }
}
