namespace Ncea.Enricher.Services.Contracts;

public interface IXmlSearchService
{
    bool IsMatchFound(string value, List<string> synonyms);
}
