using Ncea.Enricher.Models;
using System.Xml;
using System.Xml.Linq;

namespace Ncea.Enricher.Processor.Contracts;

public interface IXmlNodeService
{
    XElement CreateClassifierNode(int level, string value, List<Classifier>? classifers);
    string GetNodeValues(SearchableField field, XElement rootNode, XmlNamespaceManager nsMgr);
    XElement GetNCClassifiersParentNode(XElement rootNode, XmlNamespaceManager nsMgr);
}
