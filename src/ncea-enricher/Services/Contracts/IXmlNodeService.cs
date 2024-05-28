using Ncea.Enricher.Models;
using System.Xml.Linq;

namespace Ncea.Enricher.Services.Contracts;

public interface IXmlNodeService
{   
    string GetNodeValues(Field field, XElement rootNode);    
    void EnrichMetadataXmlWithNceaClassifiers(XElement rootNode, HashSet<Classifier> matchedClassifiers);
    XElement CreateClassifierNode(Classifier parentClassifier, List<Classifier>? childClassifers);
    XElement GetNCClassifiersParentNode(XElement rootNode);
}
