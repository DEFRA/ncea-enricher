using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputCategory : ModelInputTheme
{
    [LoadColumn(7)]
    [ColumnName(@"Label")]
    public string? CategoryL2 { get; set; }
}
