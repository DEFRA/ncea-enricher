using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelOutput
{
    [ColumnName("PredictedLabel")]
    public string? PredictedLabel { get; set; }
    [ColumnName("Score")]
    public float[]? Score { get; set; }
}