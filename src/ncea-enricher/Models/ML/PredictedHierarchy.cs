namespace Ncea.Enricher.Models.ML;

public class PredictedHierarchy
{
    public PredictedHierarchy(string theme, string themeCode, string category, string categoryCode, string subCategory)
    {
        Theme = theme;
        ThemeCode = themeCode;
        Category = category;
        CategoryCode = categoryCode;
        SubCategory = subCategory;
    }

    public string Theme { get; }
    public string ThemeCode { get; }
    public string Category { get; }
    public string CategoryCode { get; }
    public string SubCategory { get; }
}
