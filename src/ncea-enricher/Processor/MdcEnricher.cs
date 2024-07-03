using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MdcEnricher : IEnricherService
{
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly IMdcFieldConfigurationService _fieldConfigurations;
    private readonly ISearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly IXmlValidationService _xmlValidationService;
    private readonly IFeatureManager _featureManager;
    private readonly IClassifierPredictionService _classifierPredictionService;
    private readonly IClassifierVocabularyProvider _classifierVocabularyProvider;

    public MdcEnricher(ISynonymsProvider synonymsProvider,
        IMdcFieldConfigurationService fieldConfigurations,
        ISearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IFeatureManager featureManager,
        IClassifierPredictionService classifierPredictionService,
        IClassifierVocabularyProvider classifierVocabularyProvider)
    {
        _synonymsProvider = synonymsProvider;
        _fieldConfigurations = fieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _featureManager = featureManager;
        _classifierPredictionService = classifierPredictionService;
        _classifierVocabularyProvider = classifierVocabularyProvider;
    }
    public async Task<string> Enrich(string dataSource, string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        var xDoc = XDocument.Parse(mappedData);
        var rootNode = xDoc.Root!;

        var matchedClassifiers = new HashSet<ClassifierInfo>();
        if (await _featureManager.IsEnabledAsync(FeatureFlags.SynonymBasedClassificationFeature))
        {
            await FindMatchingClassifiers(rootNode, matchedClassifiers, cancellationToken);
        }
        else if (await _featureManager.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature))
        {
            await GetPredictedClassifiers(rootNode, matchedClassifiers, cancellationToken);
        }

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(rootNode, matchedClassifiers);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MdcValidationFeature))
        {
            _xmlValidationService.Validate(xDoc, dataSource, fileIdentifier);
        }

        return await Task.FromResult(xDoc.ToString());
    }

    private async Task GetPredictedClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers, CancellationToken cancellationToken)
    {
        var modelInputs = GetPredictionModelInputs(rootNode)!;

        var classifierVocabulary = await _classifierVocabularyProvider.GetAll(cancellationToken);

        var themeModelInputs = JsonConvert.DeserializeObject<ModelInputTheme>(modelInputs)!;
        var predictedThemes = _classifierPredictionService.PredictTheme(TrainedModels.Theme, themeModelInputs);
        if(predictedThemes != null && !string.IsNullOrWhiteSpace(predictedThemes.PredictedLabel))
        {
            ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 1, predictedThemes);
        }

        //var possibleCategories = _classifierPredictionService.PredictCategory(TrainedModels.Category, JsonConvert.DeserializeObject<ModelInputCategory>(modelInputs)!);
        //if (possibleCategories != null && !string.IsNullOrWhiteSpace(possibleCategories.PredictedLabel))
        //{
        //    ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 2, possibleCategories);
        //}

        //var possibleSubCategories = _classifierPredictionService.PredictSubCategory(TrainedModels.Subcategory, JsonConvert.DeserializeObject<ModelInputSubCategory>(modelInputs)!);
        //if (possibleSubCategories != null && !string.IsNullOrWhiteSpace(possibleSubCategories.PredictedLabel))
        //{
        //    ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 3, possibleSubCategories);
        //}
    }

    private static void ConsolidatePredictedClassifiers(HashSet<ClassifierInfo> matchedClassifiers, List<ClassifierInfo> classifierVocabulary, int classifierLevel, ModelOutput output)
    {
        var predictedValues = output.PredictedLabel!.Trim().Split(',').Select(x => x.Trim()).Distinct();
        var classifiers = classifierVocabulary.Where(x => x.Level == classifierLevel && predictedValues.Contains(x.Name));
        foreach (var classifier in classifiers)
        {
            if(classifierLevel > 1)
            {
                if(!matchedClassifiers.Any(x => x.Id == classifier.ParentId))
                {
                    matchedClassifiers.Add(classifierVocabulary.FirstOrDefault(x => x.Id == classifier.ParentId)!);
                }
            }
            matchedClassifiers.Add(classifier);
        }
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

    private string GetPredictionModelInputs(XElement rootNode)
    {
        var fieldValues = new Dictionary<string, string>();
        var classifierFields = _fieldConfigurations.GetFieldsForClassification();
        foreach (var classifierField in classifierFields)
        {
            var fieldValue = _xmlNodeService.GetNodeValues(classifierField, rootNode);
            fieldValues.Add(classifierField.Name.ToString(), fieldValue);
        }
        return JsonConvert.SerializeObject(fieldValues);
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
