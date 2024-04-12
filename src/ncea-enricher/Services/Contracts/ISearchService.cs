namespace Ncea.Enricher.Services.Contracts;

public interface ISearchService
{
    bool IsMatchFound(string value, List<string> synonyms);
}
