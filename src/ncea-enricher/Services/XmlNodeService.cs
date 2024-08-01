using Ncea.Enricher.Constants;
using Ncea.Enricher.Enums;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Services;

public class XmlNodeService : IXmlNodeService
{
    private readonly string _mdcSchemaLocationPath;
    private readonly XmlNamespaceManager _nsMgr;

    public XmlNodeService(IConfiguration configuration)
    {
        _mdcSchemaLocationPath = configuration.GetValue<string>("MdcSchemaLocation")!;

        _nsMgr = new XmlNamespaceManager(new NameTable());
        _nsMgr.AddNamespace("gmd", XmlNamespaces.Gmd);
        _nsMgr.AddNamespace("gco", XmlNamespaces.Gco);
        _nsMgr.AddNamespace("gmx", XmlNamespaces.Gmx);
        _nsMgr.AddNamespace("gml", XmlNamespaces.Gml);
        _nsMgr.AddNamespace("srv", XmlNamespaces.Srv);
        _nsMgr.AddNamespace("mdc", _mdcSchemaLocationPath);
    }

    public XElement CreateClassifierNode(ClassifierInfo parentClassifier, List<ClassifierInfo>? childClassifers)
    {
        XNamespace gcoNamespace = "http://www.isotc211.org/2005/gco";
        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;

        var classifier = new XElement(mdcSchemaLocation + "Classifier");

        //Create classifierValue node
        var classifierValue = new XElement(mdcSchemaLocation + "Name");
        var classifierValueCharacterString = new XElement(gcoNamespace + "CharacterString", parentClassifier.Name);
        classifierValue.Add(classifierValueCharacterString);
        classifier.Add(classifierValue);

        //Create classifierCode node
        var classifierCode = new XElement(mdcSchemaLocation + "Code");
        var classifierCodeCharacterString = new XElement(gcoNamespace + "CharacterString", parentClassifier.Id);
        classifierCode.Add(classifierCodeCharacterString);
        classifier.Add(classifierCode);

        if (childClassifers != null && childClassifers.Count != 0)
        {
            //Create child nc_Classifiers node
            var nc_ClassifiersChild = new XElement(mdcSchemaLocation + "NC_Classifiers");
            foreach (var classifer in childClassifers)
            {
                nc_ClassifiersChild.Add(CreateClassifierNode(classifer, classifer.Children));
            }

            classifier.Add(nc_ClassifiersChild);
        }

        return classifier;
    }

    public string GetNodeValues(Field field, XElement rootNode)
    {
        var value = string.Empty;

        if (field.Type == FieldType.List)
        {
            var elements = rootNode.XPathSelectElements(field.XPath, _nsMgr);
            if (elements != null && elements.Any())
            {
                var values = elements.Select(x => x.Value).ToList();
                return string.Join(",", values);
            }
        }
        else
        {
            var element = rootNode.XPathSelectElement(field.XPath, _nsMgr);
            return element != null ? element.Value : string.Empty;
        }

        return value;
    }

    public void EnrichMetadataXmlWithNceaClassifiers(XElement rootNode, HashSet<ClassifierInfo> matchedClassifiers)
    {
        var ncClassifiersParentNode = GetNCClassifiersParentNode(rootNode);
        var nceaClassifiers = BuildClassifierHierarchies(matchedClassifiers.ToList());
        foreach (var nceaClassifier in nceaClassifiers)
        {
            var element = CreateClassifierNode(nceaClassifier, nceaClassifier.Children);
            ncClassifiersParentNode.Add(element);
        }
    }    

    public XElement GetNCClassifiersParentNode(XElement rootNode)
    {
        var classifierInfo = rootNode.XPathSelectElement("//mdc:NceaClassifierInfo", _nsMgr);
        if (classifierInfo != null)
        {
            return classifierInfo.Elements().FirstOrDefault()!;
        }

        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;
        var nceaClassifierInfo = new XElement(mdcSchemaLocation + "NceaClassifierInfo");
        var nc_Classifiers = new XElement(mdcSchemaLocation + "NC_Classifiers");
        nceaClassifierInfo.Add(nc_Classifiers);
        rootNode.Add(nceaClassifierInfo);
        return nc_Classifiers;
    }

    private static List<ClassifierInfo> BuildClassifierHierarchies(List<ClassifierInfo> flattenedClassifierList)
    {
        Action<ClassifierInfo> SetChildren = null!;

        SetChildren = parent =>
        {
            parent.Children = flattenedClassifierList
                .Where(childItem => childItem.ParentId == parent.Id)
                .ToList();

            //Recursively call the SetChildren method for each child.
            parent.Children
                .ForEach(SetChildren);
        };

        //Initialize the hierarchical list to root level items
        var hierarchicalItems = flattenedClassifierList
            .Where(rootItem => rootItem.ParentId == null)
            .ToList();

        //Call the SetChildren method to set the children on each root level item.
        hierarchicalItems.ForEach(SetChildren);

        return hierarchicalItems;
    }
}
