using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputTheme : ModelInputBase
{
    [ColumnName(@"Label")]
    public string? Theme { get; set; }
}
