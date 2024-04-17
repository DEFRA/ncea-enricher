namespace Ncea.Enricher.Services.Contracts;

public interface ISearchService
{
    bool IsMatchFound(string value, List<string> synonyms);
    bool IsMatchFound(List<string> values, List<string> synonyms);
}
