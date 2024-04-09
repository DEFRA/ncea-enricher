using System.Xml.Linq;

namespace ncea.enricher.Processor.Contracts;

public interface IXmlSearchService
{
    bool IsMatchFound(XElement root, string elementPath, List<string> synonyms);
}
