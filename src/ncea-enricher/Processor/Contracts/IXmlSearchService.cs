using System.Xml.Linq;

namespace Ncea.Enricher.Processor.Contracts;

public interface IXmlSearchService
{
    bool IsMatchFound(string value, List<string> synonyms);
    bool IsMatchFound(XElement root, string elementPath, List<string> synonyms);
}
