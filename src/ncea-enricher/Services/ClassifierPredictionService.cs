using Microsoft.Extensions.ML;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Models.ML;
using Ncea.Enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Extensions;

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
        var allVocabulary = await _vocabularyProvider.GetAll(cancellationToken);
        var themes = allVocabulary.Where(x => x.Level == 1);
        var themeModelList = new List<string>();
        string themeModelListStr = string.Empty;

        PredictTheme(TrainedModels.AssetTrainedModel, Themes.Asset, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.PreassureTrainedModel, Themes.Preassure, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.BenefitTrainedModel, Themes.Benefit, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.ValuationTrainedModel, Themes.Valuation, inputData, themes, themeModelList);

        themeModelListStr = string.Join(",", themeModelList);
        return new ModelOutput() { PredictedLabel = themeModelListStr };
    }

    private void PredictTheme(string themeModel, string themeName, ModelInputTheme inputData, IEnumerable<ClassifierInfo> themes, List<string> themeModelList)
    {
        var themePrediction = _themePredictionEnginePool.Predict(themeModel, inputData);
        if (themePrediction.PredictedLabel == "1")
        {
            var theme = themes.FirstOrDefault(x => x.Id == themeName);
            themeModelList.Add($"{theme!.Id} {theme!.Name}");
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
