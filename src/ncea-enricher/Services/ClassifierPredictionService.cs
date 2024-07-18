using Microsoft.Extensions.ML;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Constants;

namespace Ncea.Enricher.Services;

public class ClassifierPredictionService : IClassifierPredictionService
{
    private readonly PredictionEnginePool<ModelInputTheme, ModelOutput> _themePredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputCategory, ModelOutput> _categoryPredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputSubCategory, ModelOutput> _subcategoryPredictionEnginePool;
    private readonly IClassifierVocabularyProvider _vocabularyProvider;

    public ClassifierPredictionService(PredictionEnginePool<ModelInputTheme, ModelOutput> themePredictionEnginePool,
        PredictionEnginePool<ModelInputCategory, ModelOutput> categoryPredictionEnginePool,
        PredictionEnginePool<ModelInputSubCategory, ModelOutput> subcategoryPredictionEnginePool,
        IClassifierVocabularyProvider vocabularyProvider)
    {
        _themePredictionEnginePool = themePredictionEnginePool;
        _categoryPredictionEnginePool = categoryPredictionEnginePool;
        _subcategoryPredictionEnginePool = subcategoryPredictionEnginePool;
        _vocabularyProvider = vocabularyProvider;
    }

    public async Task<ModelOutput> PredictTheme(ModelInputTheme inputData, CancellationToken cancellationToken)
    {
        var assetPrediction = _themePredictionEnginePool.Predict(TrainedModels.Asset, inputData);
        var pressurePrediction = _themePredictionEnginePool.Predict(TrainedModels.Preassure, inputData);
        var benefitPrediction = _themePredictionEnginePool.Predict(TrainedModels.Benefit, inputData);
        var valuationPrediction = _themePredictionEnginePool.Predict(TrainedModels.Valuation, inputData);

        var allVocabulary = await _vocabularyProvider.GetAll(cancellationToken);
        var themes = allVocabulary.Where(x => x.Level == 1);
        var themeModelList = new List<string>();

        if (assetPrediction.PredictedLabel == "1") {
            var asset = themes.FirstOrDefault(x => x.Id == Themes.Asset);
            themeModelList.Add($"{asset!.Id} {asset!.Name}");
        }

        if (pressurePrediction.PredictedLabel == "1")
        {
            var pressure = themes.FirstOrDefault(x => x.Id == Themes.Preassure);
            themeModelList.Add($"{pressure!.Id} {pressure!.Name}");
        }

        if (benefitPrediction.PredictedLabel == "1")
        {
            var benefit = themes.FirstOrDefault(x => x.Id == Themes.Benefit);
            themeModelList.Add($"{benefit!.Id} {benefit!.Name}");
        }

        if (valuationPrediction.PredictedLabel == "1")
        {
            var valuation = themes.FirstOrDefault(x => x.Id == Themes.Valuation);
            themeModelList.Add($"{valuation!.Id} {valuation!.Name}");
        }

        string themeModelListStr = string.Empty;
        if (themeModelList.Count > 0)
        {
            themeModelListStr = string.Join(",", themeModelList);
            return new ModelOutput() { PredictedLabel = themeModelListStr };
        }
        else
        {
            var themePrediction = _themePredictionEnginePool.Predict(TrainedModels.Theme, inputData);
            return themePrediction;
        }
    }

    public ModelOutput PredictCategory(string modelName, ModelInputCategory inputData)
    {
        return _categoryPredictionEnginePool.Predict(modelName, inputData);
    }

    public ModelOutput PredictSubCategory(string modelName, ModelInputSubCategory inputData)
    {
        return _subcategoryPredictionEnginePool.Predict(modelName, inputData);
    }
}
