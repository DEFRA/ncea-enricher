using Ncea.Enricher.Models.ML;

namespace Ncea.Enricher.Extensions;

public static class ClassifierStringExtensions
{
    public static IEnumerable<PredictedItem>? GetClassifierIds(this string str)
    {
        return !string.IsNullOrWhiteSpace(str) ? str.Trim()
            .Split(',')
            .Select(x => new PredictedItem(x.Trim().Substring(0, x.IndexOf(' ')), x))
            .DistinctBy(x => x.OriginalValue) : [];
    }
}
