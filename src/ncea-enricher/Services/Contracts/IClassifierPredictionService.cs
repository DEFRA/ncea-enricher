using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Services.Contracts;

public interface IClassifierPredictionService
{
    Task<ModelOutput> PredictTheme(ModelInputTheme inputData, CancellationToken cancellationToken);
    ModelOutput PredictCategory(string modelName, ModelInputCategory inputData);
    ModelOutput PredictSubCategory(string modelName, ModelInputSubCategory inputData);
}
