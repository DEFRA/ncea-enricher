using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using System.Xml;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{
    private const string InfoLogMessage1 = "Enriching metadata in-progress for DataSource: Medin, FileIdentifier: {fileIdentifier}";
    private const string InfoLogMessage2 = "Enriching metadata completed for DataSource: Medin, FileIdentifier: {fileIdentifier}";
    
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly ISearchableFieldConfigurations _searchableFieldConfigurations;
    private readonly ISearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ISynonymsProvider synonymsProvider,
        ISearchableFieldConfigurations searchableFieldConfigurations,
        ISearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        ILogger<MedinEnricher> logger)
    {
        _synonymsProvider = synonymsProvider;
        _searchableFieldConfigurations = searchableFieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _logger = logger;
    }
    public async Task<string> Enrich(string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(InfoLogMessage1, fileIdentifier);

        var searchableFieldValues = new Dictionary<string, string>();

        var xDoc = XDocument.Parse(mappedData);
        var nsMgr = _xmlNodeService.GetXmlNamespaceManager(xDoc);
        var rootNode = xDoc.Root!;

        var metadata = GetSearchableMetadataFieldValues(searchableFieldValues, nsMgr, rootNode);

        var classifierList = await _synonymsProvider.GetAll(cancellationToken);
        var classifiers = classifierList.Where(x => x.Synonyms != null).ToList();

        var matchedClassifiers = new HashSet<Classifier>();
        foreach (var classifier in classifiers.Where(x => _xmlSearchService.IsMatchFound(metadata, x.Synonyms!)))
        {
            CollectRelatedClassifiers(matchedClassifiers, classifierList, classifier);
        }

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(nsMgr, rootNode, matchedClassifiers);

        _logger.LogInformation(InfoLogMessage2, fileIdentifier);

        return await Task.FromResult(xDoc.ToString());
    }

    private List<string> GetSearchableMetadataFieldValues(Dictionary<string, string> searchableFieldValues, XmlNamespaceManager nsMgr, XElement rootNode)
    {
        var searchableFields = _searchableFieldConfigurations.GetAll();
        foreach (var searchableField in searchableFields)
        {
            var fieldValue = _xmlNodeService.GetNodeValues(searchableField, rootNode, nsMgr);
            searchableFieldValues.Add(searchableField.Name, fieldValue);
        }
        var metadata = searchableFieldValues.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList();
        return metadata;
    }

    private static void CollectRelatedClassifiers(HashSet<Classifier> matchedClassifiers, List<Classifier> classifierList, Classifier classifier)
    {
        matchedClassifiers.Add(classifier);
        while (classifier.ParentId != null)
        {
            classifier = classifierList.Single(x => x.Id == classifier.ParentId);
            matchedClassifiers.Add(classifier);
        }
    }
}
