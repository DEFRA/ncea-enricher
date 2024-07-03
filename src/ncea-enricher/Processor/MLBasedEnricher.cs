using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MLBasedEnricher : IEnricherService
{
    private readonly IFeatureManager _featureManager;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly IXmlValidationService _xmlValidationService;
    private readonly IMdcFieldConfigurationService _fieldConfigurations;
    private readonly IClassifierPredictionService _classifierPredictionService;
    private readonly IClassifierVocabularyProvider _classifierVocabularyProvider;

    public MLBasedEnricher(IFeatureManager featureManager,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IMdcFieldConfigurationService fieldConfigurations,
        IClassifierPredictionService classifierPredictionService,
        IClassifierVocabularyProvider classifierVocabularyProvider)
    {   
        _featureManager = featureManager;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _fieldConfigurations = fieldConfigurations;
        _classifierPredictionService = classifierPredictionService;
        _classifierVocabularyProvider = classifierVocabularyProvider;
    }
    public async Task<string> Enrich(string dataSource, string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        var xDoc = XDocument.Parse(mappedData);
        var rootNode = xDoc.Root!;

        var matchedClassifiers = new HashSet<ClassifierInfo>();
        if (await _featureManager.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature))
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

        var possibleCategories = _classifierPredictionService.PredictCategory(TrainedModels.Category, JsonConvert.DeserializeObject<ModelInputCategory>(modelInputs)!);
        if (possibleCategories != null && !string.IsNullOrWhiteSpace(possibleCategories.PredictedLabel))
        {
            ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 2, possibleCategories);
        }

        var possibleSubCategories = _classifierPredictionService.PredictSubCategory(TrainedModels.Subcategory, JsonConvert.DeserializeObject<ModelInputSubCategory>(modelInputs)!);
        if (possibleSubCategories != null && !string.IsNullOrWhiteSpace(possibleSubCategories.PredictedLabel))
        {
            ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 3, possibleSubCategories);
        }
    }

    private static void ConsolidatePredictedClassifiers(HashSet<ClassifierInfo> matchedClassifiers, List<ClassifierInfo> classifierVocabulary, int classifierLevel, ModelOutput output)
    {
        var predictedValues = output.PredictedLabel!
            .Trim()
            .Split(',')
            .Select(x => x.Trim().Substring(0, x.IndexOf(' ')))
            .Distinct();

        var classifiers = classifierVocabulary.Where(x => x.Level == classifierLevel && predictedValues.Contains(x.Id));
        foreach (var classifier in classifiers)
        {
            matchedClassifiers.Add(classifier);           
            
            var parentId = classifier.ParentId;
            while (parentId != null && !matchedClassifiers.Any(x => x.Id == parentId))
            {
                var parentClassifier = classifierVocabulary.Single(x => x.Id == parentId);
                matchedClassifiers.Add(parentClassifier);

                parentId = parentClassifier.ParentId;
            }
        }
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
}
