﻿using Microsoft.FeatureManagement;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Extensions;
using Ncea.Enricher.Models;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

[ExcludeFromCodeCoverage]
public class MLBasedEnricher : IEnricherService
{
    private readonly string[] _categoriesToBeExcludedForSubCategoryPredictions = ["lv2-009", "lv2-010"];

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

        await GetPredictedClassifiers(rootNode, matchedClassifiers, fileIdentifier, dataSource, cancellationToken);

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(rootNode, matchedClassifiers);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MdcValidationFeature))
        {
            _xmlValidationService.Validate(xDoc, dataSource, fileIdentifier);
        }

        return await Task.FromResult(xDoc.ToString());
    }

    private async Task GetPredictedClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers, string fileIdentifier, string dataSource, CancellationToken cancellationToken)
    {
        var classifierVocabulary = await _classifierVocabularyProvider.GetAll(cancellationToken);

        var modelInputs = GetPredictionModelInputs(rootNode)!;

        var predictedThemesList = await _classifierPredictionService.PredictTheme(JsonConvert.DeserializeObject<ModelInputTheme>(modelInputs)!, cancellationToken);
        var predictedThemes = predictedThemesList
                              .PredictedLabel!
                              .GetClassifierIds();

        var predictedCategories = new List<PredictedItem>();
        var predictedThemeCategories = new List<PredictedHierarchy>();
        PredictCategories(modelInputs, predictedThemes, predictedCategories, predictedThemeCategories);

        var predictedSubCategories = new List<PredictedItem>();
        PredictSubCategories(modelInputs, predictedCategories, predictedThemeCategories, predictedSubCategories);

        List<string> missingParentClassifiers = [];

        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 1, predictedThemes, missingParentClassifiers);
        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 2, predictedCategories, missingParentClassifiers);
        ConsolidatePredictedClassifiers(matchedClassifiers, classifierVocabulary, 3, predictedSubCategories, missingParentClassifiers);

        var predictedClassifiersLogText = $"ML Predicted classifiers for Datasource : {dataSource} | " +
                $"FileIdentifier :{fileIdentifier} | " +
                $"Themes :{string.Join(",", predictedThemes != null ? predictedThemes.Select(x => x.OriginalValue).Distinct() : [])} | " +
                $"Categories :{string.Join(",", predictedCategories.Select(x => x.OriginalValue).Distinct())} | " +
                $"SubCategories :{string.Join(",", predictedSubCategories.Select(x => x.OriginalValue).Distinct())}";

        _logger.LogWarning("{predictedClassifiersLogText} | Missing ParentIds : {missingparentIds}",
            predictedClassifiersLogText,
            string.Join(", ", missingParentClassifiers));
    }

    private void PredictSubCategories(string modelInputs, List<PredictedItem> predictedCategories, List<PredictedHierarchy> predictedThemeCategories, List<PredictedItem> predictedSubCategories)
    {
        if (predictedCategories.Count > 0)
        {
            var subCategoryInput = JsonConvert.DeserializeObject<ModelInputSubCategory>(modelInputs)!;
            foreach (var predictedThemeCategory in predictedThemeCategories)
            {                
                subCategoryInput.Theme = !string.IsNullOrWhiteSpace(predictedThemeCategory.Theme) ? predictedThemeCategory.Theme : null;                

                if (!string.IsNullOrWhiteSpace(predictedThemeCategory.CategoryCode) && !_categoriesToBeExcludedForSubCategoryPredictions.Contains(predictedThemeCategory.CategoryCode))
                {
                    subCategoryInput.CategoryL2 = predictedThemeCategory.Category;

                    var subCategories = _classifierPredictionService.PredictSubCategory(predictedThemeCategory.CategoryCode, subCategoryInput)
                    .PredictedLabel!
                    .GetClassifierIds();

                    if (subCategories != null && subCategories.Any())
                    {
                        predictedSubCategories.AddRange(subCategories);
                    }
                }                
            }
        }
    }

    private void PredictCategories(string modelInputs, IEnumerable<PredictedItem>? predictedThemes, List<PredictedItem> predictedCategories, List<PredictedHierarchy> predictedThemeCategories)
    {
        if (predictedThemes != null && predictedThemes.Any())
        {
            var categoryInput = JsonConvert.DeserializeObject<ModelInputCategory>(modelInputs)!;
            foreach (var predictedTheme in predictedThemes)
            {
                var originalValue = predictedTheme.OriginalValue;
                var codeValue = predictedTheme.Code;
                categoryInput.Theme = !string.IsNullOrWhiteSpace(originalValue) ? originalValue : null;

                if (!string.IsNullOrWhiteSpace(predictedTheme.Code))
                {
                    var categories = _classifierPredictionService.PredictCategory(codeValue, categoryInput)
                        .PredictedLabel!
                        .GetClassifierIds();

                    if (categories != null && categories.Any())
                    {
                        predictedCategories.AddRange(categories);
                        predictedThemeCategories.AddRange(categories.Select(x => new PredictedHierarchy(originalValue, codeValue, x.OriginalValue, x.Code, string.Empty)));
                    }
                }
            }
        }
    }

    private static void ConsolidatePredictedClassifiers(HashSet<ClassifierInfo> matchedClassifiers, 
        List<ClassifierInfo> classifierVocabulary, 
        int classifierLevel, 
        IEnumerable<PredictedItem>? predictedClassifierIds,
        List<string> missingParentClassifiers)
    {
        if (predictedClassifierIds != null && predictedClassifierIds.Any())
        {
            
            var classifiers = classifierVocabulary.Where(x => x.Level == classifierLevel && predictedClassifierIds.Select(y => y.Code).Contains(x.Id));
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
