using Microsoft.Extensions.ML;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Extensions;
using static System.Formats.Asn1.AsnWriter;

namespace Ncea.Enricher.Services;

public class ClassifierPredictionService : IClassifierPredictionService
{
    private readonly PredictionEnginePool<ModelInputTheme, ModelOutput> _themePredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputCategory, ModelOutput> _categoryPredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputSubCategory, ModelOutput> _subcategoryPredictionEnginePool;
    private readonly IClassifierVocabularyProvider _vocabularyProvider;
    private readonly string _assetConfidenceThreshold;
    private readonly string _pressureConfidenceThreshold;
    private readonly string _benefitConfidenceThreshold;
    private readonly string _valuationConfidenceThreshold;
    private readonly string _categoryConfidenceThreshold;
    private readonly string _subCategoryConfidenceThreshold;

    public ClassifierPredictionService(PredictionEnginePool<ModelInputTheme, ModelOutput> themePredictionEnginePool,
        PredictionEnginePool<ModelInputCategory, ModelOutput> categoryPredictionEnginePool,
        PredictionEnginePool<ModelInputSubCategory, ModelOutput> subcategoryPredictionEnginePool,
        IClassifierVocabularyProvider vocabularyProvider,
        IConfiguration configuration)
    {
        _themePredictionEnginePool = themePredictionEnginePool;
        _categoryPredictionEnginePool = categoryPredictionEnginePool;
        _subcategoryPredictionEnginePool = subcategoryPredictionEnginePool;
        _vocabularyProvider = vocabularyProvider;
        _assetConfidenceThreshold = configuration.GetValue<string>("AssetConfidenceThreshold")!;
        _pressureConfidenceThreshold = configuration.GetValue<string>("PressureConfidenceThreshold")!;
        _benefitConfidenceThreshold = configuration.GetValue<string>("BenefitConfidenceThreshold")!;
        _valuationConfidenceThreshold = configuration.GetValue<string>("ValuationConfidenceThreshold")!;
        _categoryConfidenceThreshold = configuration.GetValue<string>("CategoryConfidenceThreshold")!;
        _subCategoryConfidenceThreshold = configuration.GetValue<string>("SubCategoryConfidenceThreshold")!;
    }

    public async Task<ModelOutput> PredictTheme(ModelInputTheme inputData, CancellationToken cancellationToken)
    {
        var allVocabulary = await _vocabularyProvider.GetAll(cancellationToken);
        var themes = allVocabulary.Where(x => x.Level == 1);
        var themeModelList = new List<string>();
        string themeModelListStr = string.Empty;
        

        PredictTheme(TrainedModels.AssetTrainedModel, Themes.Asset, inputData, themes, themeModelList, _assetConfidenceThreshold);
        PredictTheme(TrainedModels.PreassureTrainedModel, Themes.Preassure, inputData, themes, themeModelList, _pressureConfidenceThreshold);
        PredictTheme(TrainedModels.BenefitTrainedModel, Themes.Benefit, inputData, themes, themeModelList, _benefitConfidenceThreshold);
        PredictTheme(TrainedModels.ValuationTrainedModel, Themes.Valuation, inputData, themes, themeModelList, _valuationConfidenceThreshold);

        themeModelListStr = string.Join(",", themeModelList);
        return new ModelOutput() { PredictedLabel = themeModelListStr };
    }

    private void PredictTheme(string themeModel, string themeName, ModelInputTheme inputData, IEnumerable<ClassifierInfo> themes, List<string> themeModelList, string themeConfidenceThreshold)
    {
        var themePrediction = _themePredictionEnginePool.Predict(themeModel, inputData);                

        if (themePrediction.PredictedLabel == "1")
        {
            var confidenceThreshold = float.Parse(themeConfidenceThreshold);
            var metConfidence = Array.Exists(themePrediction.Score!, score => score > confidenceThreshold);

            if (metConfidence)
            {
                var theme = themes.FirstOrDefault(x => x.Id == themeName);
                themeModelList.Add($"{theme!.Id} {theme!.Name}");
            }
        }
    }

    public ModelOutput PredictCategory(string modelName, ModelInputCategory inputData)
    {
        var _prediction = _categoryPredictionEnginePool.Predict(modelName, inputData);
        var confidenceThreshold = float.Parse(_categoryConfidenceThreshold);
        return CheckConfidenceAndReturnPrediction(_prediction, confidenceThreshold);
    }

    public ModelOutput PredictSubCategory(string modelName, ModelInputSubCategory inputData)
    {
        var _prediction = _subcategoryPredictionEnginePool.Predict(modelName, inputData);
        var confidenceThreshold = float.Parse(_subCategoryConfidenceThreshold);
        return CheckConfidenceAndReturnPrediction(_prediction, confidenceThreshold);
    }

    private static ModelOutput CheckConfidenceAndReturnPrediction(ModelOutput _prediction, float confidenceThreshold)
    {
        var metConfidence = Array.Exists(_prediction.Score!, score => score > confidenceThreshold);

        if (!metConfidence)
        {
            _prediction.PredictedLabel = string.Empty;
        }
        return _prediction;
    }
}
