using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using System.Xml;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MdcEnricher : IEnricherService
{
    private const string InfoLogMessage1 = "Enriching metadata in-progress for DataSource: Medin, FileIdentifier: {fileIdentifier}";
    private const string InfoLogMessage2 = "Enriching metadata completed for DataSource: Medin, FileIdentifier: {fileIdentifier}";
    
    private readonly string _mdcSchemaLocationPath;
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly ISearchableFieldConfigurations _searchableFieldConfigurations;
    private readonly ISearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly IXmlValidationService _xmlValidationService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<MdcEnricher> _logger;

    public MdcEnricher(ISynonymsProvider synonymsProvider,
        ISearchableFieldConfigurations searchableFieldConfigurations,
        ISearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IFeatureManager featureManager,        
        IConfiguration configuration,
        ILogger<MdcEnricher> logger)
    {
        _mdcSchemaLocationPath = configuration.GetValue<string>("MdcSchemaLocation")!;

        _synonymsProvider = synonymsProvider;
        _searchableFieldConfigurations = searchableFieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _featureManager = featureManager;
        _logger = logger;
    }
    public async Task<string> Enrich(string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(InfoLogMessage1, fileIdentifier);        

        var xDoc = XDocument.Parse(mappedData);
        var nsMgr = _xmlNodeService.GetXmlNamespaceManager(xDoc);
        var rootNode = xDoc.Root!;

        var matchedClassifiers = new HashSet<Classifier>();

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MetadataEnrichmentFeature))
        {
            await FindMatchingClassifiers(nsMgr, rootNode, matchedClassifiers, cancellationToken);
        }

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(nsMgr, rootNode, matchedClassifiers);

        _xmlValidationService.Validate(xDoc);
        
        _logger.LogInformation(InfoLogMessage2, fileIdentifier);

        return await Task.FromResult(xDoc.ToString());
    }

    private async Task FindMatchingClassifiers(XmlNamespaceManager nsMgr, XElement rootNode, HashSet<Classifier> matchedClassifiers, CancellationToken cancellationToken)
    {
        var searchableFieldValues = new Dictionary<string, string>();

        var metadata = GetSearchableMetadataFieldValues(searchableFieldValues, nsMgr, rootNode);

        var classifierList = await _synonymsProvider.GetAll(cancellationToken);
        var classifiers = classifierList.Where(x => x.Synonyms != null).ToList();

        foreach (var classifier in classifiers.Where(x => _xmlSearchService.IsMatchFound(metadata, x.Synonyms!)))
        {
            CollectRelatedClassifiers(matchedClassifiers, classifierList, classifier);
        }
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
