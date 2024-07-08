namespace Ncea.Enricher.Extensions;

public static class ClassifierStringExtensions
{
    public static IEnumerable<string>? GetClassifierIds(this string str)
    {
        return (str != null) ? str.Trim()
            .Split(',')
            .Select(x => x.Trim().Substring(0, x.IndexOf(' ')))
            .Distinct() : null;
    }
}
