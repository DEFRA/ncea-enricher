﻿using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Services.Contracts;

public interface IClassifierPredictionService
{
    ModelOutput PredictTheme(string modelName, ModelInputTheme inputData);
    ModelOutput PredictCategory(string modelName, ModelInputCategory inputData);
    ModelOutput PredictSubCategory(string modelName, ModelInputSubCategory inputData);
}
