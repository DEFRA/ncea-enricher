using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public class ModelInputBase
{
    [LoadColumn(0)]
    [ColumnName(@"Topics")]
    public string? Topics { get; set; }

    [LoadColumn(1)]
    [ColumnName(@"Keywords")]
    public string? Keywords { get; set; }

    [LoadColumn(2)]
    [ColumnName(@"Title")]
    public string? Title { get; set; }

    [LoadColumn(3)]
    [ColumnName(@"AltTitle")]
    public string? AltTitle { get; set; }

    [LoadColumn(4)]
    [ColumnName(@"Abstract")]
    public string? Abstract { get; set; }

    [LoadColumn(5)]
    [ColumnName(@"Lineage")]
    public string? Lineage { get; set; }

    [LoadColumn(6)]
    [ColumnName(@"Label")]
    public string? Theme { get; set; }
}
