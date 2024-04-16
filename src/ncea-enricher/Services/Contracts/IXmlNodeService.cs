using Ncea.Enricher.Models;
using System.Xml;
using System.Xml.Linq;

namespace Ncea.Enricher.Services.Contracts;

public interface IXmlNodeService
{
    XmlNamespaceManager GetXmlNamespaceManager(XDocument xDoc);    
    string GetNodeValues(SearchableField field, XElement rootNode, XmlNamespaceManager nsMgr);    
    void EnrichMetadataXmlWithNceaClassifiers(XmlNamespaceManager nsMgr, XElement rootNode, HashSet<Classifier> matchedClassifiers);
    XElement CreateClassifierNode(int level, string value, List<Classifier>? classifers);
    XElement GetNCClassifiersParentNode(XElement rootNode, XmlNamespaceManager nsMgr);
}
