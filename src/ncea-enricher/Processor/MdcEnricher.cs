﻿using Microsoft.FeatureManagement;
using ncea.enricher.Services.Contracts;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;
using Ncea.Enricher.Services.Contracts;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MdcEnricher : IEnricherService
{
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly ISearchableFieldConfigurations _searchableFieldConfigurations;
    private readonly ISearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly IXmlValidationService _xmlValidationService;
    private readonly IFeatureManager _featureManager;
    private readonly IClassifierPredictionService _classifierPredictionService;

    public MdcEnricher(ISynonymsProvider synonymsProvider,
        ISearchableFieldConfigurations searchableFieldConfigurations,
        ISearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        IXmlValidationService xmlValidationService,
        IFeatureManager featureManager,
        IClassifierPredictionService classifierPredictionService)
    {
        _synonymsProvider = synonymsProvider;
        _searchableFieldConfigurations = searchableFieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _xmlValidationService = xmlValidationService;
        _featureManager = featureManager;
        _classifierPredictionService = classifierPredictionService;
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
            GetPredictedClassifiers(rootNode, matchedClassifiers);
        }

        _xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(rootNode, matchedClassifiers);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.MdcValidationFeature))
        {
            _xmlValidationService.Validate(xDoc, dataSource, fileIdentifier);
        }

        return await Task.FromResult(xDoc.ToString());
    }

    private void GetPredictedClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers)
    {

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
        var searchableFields = _searchableFieldConfigurations.GetAll();
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
