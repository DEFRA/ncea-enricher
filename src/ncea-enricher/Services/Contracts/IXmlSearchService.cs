using System.Xml.Linq;

namespace Ncea.Enricher.Services.Contracts;

public interface IXmlSearchService
{
    bool IsMatchFound(string value, List<string> synonyms);
    bool IsMatchFound(XElement root, string elementPath, List<string> synonyms);
}
