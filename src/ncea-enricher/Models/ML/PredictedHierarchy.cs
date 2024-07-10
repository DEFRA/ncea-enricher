namespace Ncea.Enricher.Models.ML;

public class PredictedHierarchy
{
    public PredictedHierarchy(string theme, string category, string subCategory)
    {
        Theme = theme;
        Category = category;
        SubCategory = subCategory;
    }

    public string Theme { get; }
    public string Category { get; }
    public string SubCategory { get; }
}
