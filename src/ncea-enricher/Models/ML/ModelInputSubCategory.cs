using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputSubCategory : ModelInputCategory
{
    [LoadColumn(8)]
    [ColumnName(@"Label")]
    public string? SubCategoryL3 { get; set; }
}
