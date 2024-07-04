using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputSubCategory : ModelInputBase
{
    [ColumnName(@"Theme")]
    public string? Theme { get; set; }

    [ColumnName(@"CategoryL2")]
    public string? CategoryL2 { get; set; }

    [ColumnName(@"Label")]
    public string? SubCategoryL3 { get; set; }
}
