using Microsoft.Extensions.ML;
using ncea.enricher.Services.Contracts;
using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Services;

public class ClassifierPredictionService : IClassifierPredictionService
{
    private readonly PredictionEnginePool<ModelInputTheme, ModelOutput> _themePredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputCategory, ModelOutput> _categoryPredictionEnginePool;
    private readonly PredictionEnginePool<ModelInputSubCategory, ModelOutput> _subcategoryPredictionEnginePool;

    public ClassifierPredictionService(PredictionEnginePool<ModelInputTheme, ModelOutput> themePredictionEnginePool,
        PredictionEnginePool<ModelInputCategory, ModelOutput> categoryPredictionEnginePool,
        PredictionEnginePool<ModelInputSubCategory, ModelOutput> subcategoryPredictionEnginePool)
    {
        _themePredictionEnginePool = themePredictionEnginePool;
        _categoryPredictionEnginePool = categoryPredictionEnginePool;
        _subcategoryPredictionEnginePool = subcategoryPredictionEnginePool;
    }

    public ModelOutput PredictTheme(string modelName, ModelInputTheme inputData)
    {
        return _themePredictionEnginePool.Predict(modelName, inputData);
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
