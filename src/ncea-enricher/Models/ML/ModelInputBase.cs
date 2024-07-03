using Microsoft.ML.Data;

namespace Ncea.Enricher.Models.ML;

public abstract class ModelInputBase
{
    [ColumnName(@"Topics")]
    public string? Topics { get; set; }
    
    [ColumnName(@"Keywords")]
    public string? Keywords { get; set; }
    
    [ColumnName(@"Title")]
    public string? Title { get; set; }
    
    [ColumnName(@"AltTitle")]
    public string? AltTitle { get; set; }
    
    [ColumnName(@"Abstract")]
    public string? Abstract { get; set; }
    
    [ColumnName(@"Lineage")]
    public string? Lineage { get; set; }
}
