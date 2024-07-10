using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class SynonymBasedEnricher : IEnricherService
{
    private readonly IFeatureManager _featureManager;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly IXmlValidationService _xmlValidationService;
    private readonly IMdcFieldConfigurationService _fieldConfigurations;
    private readonly ISearchService _xmlSearchService;
    private readonly ISynonymsProvider _synonymsProvider;

    public SynonymBasedEnricher(IFeatureManager featureManager,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IMdcFieldConfigurationService fieldConfigurations,
        ISearchService xmlSearchService, 
        ISynonymsProvider synonymsProvider)
    {
        _featureManager = featureManager;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _fieldConfigurations = fieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _synonymsProvider = synonymsProvider;
    }

    public async Task<string> Enrich(string dataSource, string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        var xDoc = XDocument.Parse(mappedData);
        var rootNode = xDoc.Root!;

        var matchedClassifiers = new HashSet<ClassifierInfo>();

        await FindMatchingClassifiers(rootNode, matchedClassifiers, cancellationToken);

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(rootNode, matchedClassifiers);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MdcValidationFeature))
        {
            _xmlValidationService.Validate(xDoc, dataSource, fileIdentifier);
        }

        return await Task.FromResult(xDoc.ToString());
    }

    private async Task FindMatchingClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers, CancellationToken cancellationToken)
    {
        var searchableFieldValues = new Dictionary<string, string>();

        var metadata = GetSearchableMetadataFieldValues(searchableFieldValues, rootNode);

        var classifierList = await _synonymsProvider.GetAll(cancellationToken);
        var classifiers = classifierList.Where(x => x.Synonyms != null).ToList();

        foreach (var classifier in classifiers.Where(x => _xmlSearchService.IsMatchFound(metadata, x.Synonyms!)))
        {
            CollectRelatedClassifiers(matchedClassifiers, classifierList, classifier);
        }
    }

    private List<string> GetSearchableMetadataFieldValues(Dictionary<string, string> searchableFieldValues, XElement rootNode)
    {
        var searchableFields = _fieldConfigurations.GetFieldsForClassification();
        foreach (var searchableField in searchableFields)
        {
            var fieldValue = _xmlNodeService.GetNodeValues(searchableField, rootNode);
            searchableFieldValues.Add(searchableField.Name.ToString(), fieldValue);
        }
        var metadata = searchableFieldValues.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList();
        return metadata;
    }    

    private static void CollectRelatedClassifiers(HashSet<ClassifierInfo> matchedClassifiers, List<ClassifierInfo> classifierList, ClassifierInfo classifier)
    {
        matchedClassifiers.Add(classifier);
        while (classifier.ParentId != null)
        {
            classifier = classifierList.Single(x => x.Id == classifier.ParentId);
            matchedClassifiers.Add(classifier);
        }
    }
}
