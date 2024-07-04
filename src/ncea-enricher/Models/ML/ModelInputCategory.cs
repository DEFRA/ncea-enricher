using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputCategory : ModelInputBase
{
    [ColumnName(@"Theme")]
    public string? Theme { get; set; }

    [ColumnName(@"Label")]
    public string? CategoryL2 { get; set; }
}
