using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Extensions;
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
    private readonly ILogger _logger;

    public MLBasedEnricher(IFeatureManager featureManager,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IMdcFieldConfigurationService fieldConfigurations,
        IClassifierPredictionService classifierPredictionService,
        IClassifierVocabularyProvider classifierVocabularyProvider,
        ILogger<MLBasedEnricher> logger)
    {
        _featureManager = featureManager;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _fieldConfigurations = fieldConfigurations;
        _classifierPredictionService = classifierPredictionService;
        _classifierVocabularyProvider = classifierVocabularyProvider;
        _logger = logger;
    }
    public async Task<string> Enrich(string dataSource, string fileIdentifier, string mappedData, CancellationToken cancellationToken = default)
    {
        var xDoc = XDocument.Parse(mappedData);
        var rootNode = xDoc.Root!;

        var matchedClassifiers = new HashSet<ClassifierInfo>();
        if (await _featureManager.IsEnabledAsync(FeatureFlags.MLBasedClassificationFeature))
        {
            await GetPredictedClassifiers(rootNode, matchedClassifiers, fileIdentifier, cancellationToken);
        }

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(rootNode, matchedClassifiers);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MdcValidationFeature))
        {
            _xmlValidationService.Validate(xDoc, dataSource, fileIdentifier);
        }

        return await Task.FromResult(xDoc.ToString());
    }

    private async Task GetPredictedClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers, string fileIdentifier, CancellationToken cancellationToken)
    {
        var modelInputs = GetPredictionModelInputs(rootNode)!;

        var classifierVocabulary = await _classifierVocabularyProvider.GetAll(cancellationToken);

        var predictedThemes = _classifierPredictionService.PredictTheme(TrainedModels.Theme, JsonConvert.DeserializeObject<ModelInputTheme>(modelInputs)!)
            .PredictedLabel!
            .GetClassifierIds();
        var predictedCategories = _classifierPredictionService.PredictCategory(TrainedModels.Category, JsonConvert.DeserializeObject<ModelInputCategory>(modelInputs)!)
            .PredictedLabel!
            .GetClassifierIds();
        var predictedSubCategories = _classifierPredictionService.PredictSubCategory(TrainedModels.SubCategory, JsonConvert.DeserializeObject<ModelInputSubCategory>(modelInputs)!)
            .PredictedLabel!
            .GetClassifierIds();        
        
        List<string> missingParentClassifiers = [];

        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 1, predictedThemes, missingParentClassifiers);
        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 2, predictedCategories, missingParentClassifiers);
        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 3, predictedSubCategories, missingParentClassifiers);

        if (missingParentClassifiers.Count > 0)
        {
            var predictedClassifiersLogText = $"Predicted classifiers for FileIdentifier : {fileIdentifier} | " +
            $"Themes: { string.Join(", ", predictedThemes ?? []) } | " +
            $"Categories: {string.Join(", ", predictedCategories ?? []) } | " +
            $"SubCategories: {string.Join(", ", predictedSubCategories ?? []) }";

            _logger.LogWarning("Classifier Integerity Issues detected : {predictedClassifiersLogText}, Missing ParentIds : {missingparentIds}",
                predictedClassifiersLogText,
                string.Join(", ", missingParentClassifiers));
        }
    }

    private static void ConsolidatePredictedClassifiers(HashSet<ClassifierInfo> matchedClassifiers, 
        List<ClassifierInfo> classifierVocabulary, 
        int classifierLevel, 
        IEnumerable<string>? predictedClassifierIds,
        List<string> missingParentClassifiers)
    {
        if (predictedClassifierIds != null && predictedClassifierIds.Any())
        {
            
            var classifiers = classifierVocabulary.Where(x => x.Level == classifierLevel && predictedClassifierIds.Contains(x.Id));
            foreach (var classifier in classifiers)
            {
                matchedClassifiers.Add(classifier);

                var parentId = classifier.ParentId;
                while (parentId != null && !matchedClassifiers.Any(x => x.Id == parentId))
                {
                    missingParentClassifiers.Add(parentId);
                    var parentClassifier = classifierVocabulary.Single(x => x.Id == parentId);
                    matchedClassifiers.Add(parentClassifier);

                    parentId = parentClassifier.ParentId;
                }
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
