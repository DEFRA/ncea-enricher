using Ncea.Enricher.Models;
using System.Xml.Linq;

namespace Ncea.Enricher.Services.Contracts;

public interface IXmlNodeService
{   
    string GetNodeValues(Field field, XElement rootNode);    
    void EnrichMetadataXmlWithNceaClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers);
    XElement CreateClassifierNode(ClassifierInfo parentClassifier, List<ClassifierInfo>? childClassifers);
    XElement GetNCClassifiersParentNode(XElement rootNode);
}
