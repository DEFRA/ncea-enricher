using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Azure;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Ncea.Enricher.BusinessExceptions;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Utils;
using Ncea.Enricher.Constants;
using System.Text;
using System.Text.Json;
using Ncea.Enricher.Models;
using Ncea.Enricher.Infrastructure.Models.Requests;
using Ncea.Enricher.Infrastructure.Contracts;
using System.Text.Json.Serialization;
using Ncea.Enricher.Enums;
using Ncea.Harvester.Services.Contracts;
using Microsoft.Azure.Amqp.Framing;

namespace Ncea.Enricher.Services;

public class OrchestrationService : IOrchestrationService
{
    private const string ProcessorErrorMessage = "Enricher Exception | Error in processing message in ncea-enricher service";

    private readonly string _fileShareName;
    private readonly IBackUpService _backupService;
    private readonly IBlobService _blobService;
    private readonly ServiceBusProcessor _processor;
    private readonly IEnricherService _mdcEnricherSerivice;
    private readonly ILogger<OrchestrationService> _logger;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private string? _fileIdentifier;
    private readonly string _mapperStagingContainerSuffix;

    public OrchestrationService(IConfiguration configuration,
        IBackUpService backupService,
        IBlobService blobService,
        IAzureClientFactory<ServiceBusProcessor> serviceBusProcessorFactory,
        IEnricherService mdcEnricherSerivice,
        ILogger<OrchestrationService> logger)
    {
        var mapperQueueName = configuration.GetValue<string>("MapperQueueName");
        _fileShareName = configuration.GetValue<string>("FileShareName")!;
        _mapperStagingContainerSuffix = configuration.GetValue<string>("MapperStagingContainerSuffix")!;

        _processor = serviceBusProcessorFactory.CreateClient(mapperQueueName);
        _mdcEnricherSerivice = mdcEnricherSerivice;
        _backupService = backupService;
        _blobService = blobService;
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
        var dataSource = string.Empty;
        var fileIdentifier = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(args.Message.Body.ToString()))
            {
                throw new ArgumentException("Mappeed-queue message body should not be empty");
            }

            var enricherDirectory = dataSource;
            var backupEnricherDirectory = $"{dataSource}-backup";
            var newEnricherDirectory = $"{dataSource}-new";
            var body = Encoding.UTF8.GetString(args.Message.Body);
            var mdcMappedRecord = JsonSerializer.Deserialize<MdcMappedRecordMessage>(body, _serializerOptions)!;
            dataSource = mdcMappedRecord.DataSource.ToString();

            if (mdcMappedRecord.MessageType == MessageType.Start)
            {
                _backupService.CreateDirectory(newEnricherDirectory);

                _logger.LogInformation("Enricher summary | Metadata enrichment started for DataSource : {dataSource}.", dataSource);
            }
            else if (mdcMappedRecord.MessageType == MessageType.End)
            {
                
                _backupService.MoveFiles(enricherDirectory, backupEnricherDirectory);                
                _backupService.MoveFiles(newEnricherDirectory, enricherDirectory);

                _logger.LogInformation("Enricher summary | Metadata enrichment ended for DataSource : {dataSource}.", dataSource);
            }
            else
            {
                fileIdentifier = mdcMappedRecord.FileIdentifier;

                var dataSourceNameInLowerCase = dataSource.ToLowerInvariant();
                var fileName = string.Concat(fileIdentifier, ".xml");
                var mapperContainerName = $"{dataSourceNameInLowerCase}-{_mapperStagingContainerSuffix}";

                var mdcMappedData = await _blobService.GetContentAsync(new GetBlobContentRequest(fileName, mapperContainerName), args.CancellationToken);

                _fileIdentifier = GetFileIdentifier(mdcMappedData)!;
                var enrichedMetadata = await _mdcEnricherSerivice.Enrich(dataSource, _fileIdentifier, mdcMappedData);

                await SaveEnrichedXmlAsync(enrichedMetadata, dataSourceNameInLowerCase);

                await _blobService.DeleteBlobAsync(new DeleteBlobRequest(fileName, mapperContainerName), args.CancellationToken);
                _logger.LogInformation("Enricher summary | Metadata enrichment completed for DataSource : {dataSource}, FileIdentifier : {fileIdentifier}", dataSource, fileIdentifier);
            }
            

            await args.CompleteMessageAsync(args.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleException(args, ex, new EnricherArgumentException(ex.Message, ex));
        }
        catch (RequestFailedException ex)
        {
            var errorMessage = $"Error occured while reading the synonyms file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new BlobStorageNotAccessibleException(errorMessage, ex));
        }
        catch (DirectoryNotFoundException ex)
        {
            var errorMessage = $"Error occured while saving the xml file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new FileShareNotFoundException(errorMessage, ex));
        }
        catch (XmlSchemaValidationException ex)
        {
            var errorMessage = $"Error occured while validating enriched xml file during enrichment process for Data source: {dataSource}, file-id: {_fileIdentifier}";
            await HandleException(args, ex, new XmlValidationException(errorMessage, ex));
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
        var newDatasourceConatinerName = $"{dataSource}-new";
        var filePath = Path.Combine(_fileShareName, newDatasourceConatinerName, fileName);
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
        nsMgr.AddNamespace("gmd", XmlNamespaces.Gmd);
        nsMgr.AddNamespace("gco", XmlNamespaces.Gco);
        nsMgr.AddNamespace("gmx", XmlNamespaces.Gmx);

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
        _logger.LogError(businessException, ProcessorErrorMessage);
        CustomLogger.LogErrorMessage(_logger, businessException.Message, ex);
        await args.AbandonMessageAsync(args.Message);
        throw businessException;
    }
}
