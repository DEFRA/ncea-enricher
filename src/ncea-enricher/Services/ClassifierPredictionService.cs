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

        PredictTheme(TrainedModels.Asset, Themes.Asset, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.Preassure, Themes.Preassure, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.Benefit, Themes.Benefit, inputData, themes, themeModelList);
        PredictTheme(TrainedModels.Valuation, Themes.Valuation, inputData, themes, themeModelList);

        string themeModelListStr = string.Empty;

        if (themeModelList.Count == 0)
        {
            var themePrediction = _themePredictionEnginePool.Predict(TrainedModels.Theme, inputData);
            var predictedThemes = themePrediction.PredictedLabel!.GetClassifierIds();

            foreach(var predictedTheme in predictedThemes!)
            {
                var theme = themes.FirstOrDefault(x => x.Id == predictedTheme.Code);
                themeModelList.Add($"{theme!.Id} {theme!.Name}");
            }
        }

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
