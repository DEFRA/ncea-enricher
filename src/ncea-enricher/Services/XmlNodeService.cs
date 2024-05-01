using ncea.enricher.Constants;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Services;

public class XmlNodeService : IXmlNodeService
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";

    private readonly string _mdcSchemaLocationPath;

    public XmlNodeService(IConfiguration configuration)
    {
        _mdcSchemaLocationPath = configuration.GetValue<string>("MdcSchemaLocation")!;
    }

    public XElement CreateClassifierNode(Classifier parentClassifier, List<Classifier>? childClassifers)
    {
        XNamespace gcoNamespace = "http://www.isotc211.org/2005/gco";
        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;

        var classifier = new XElement(mdcSchemaLocation + "classifier");
        var classifierTypeValue = (ClassifierTypes)parentClassifier.Level;

        //Create classifierType node
        var classifierType = new XElement(mdcSchemaLocation + "classifierType");
        var classifierTypeCharacterString = new XElement(gcoNamespace + "CharacterString", classifierTypeValue.ToString());
        classifierType.Add(classifierTypeCharacterString);
        classifier.Add(classifierType);

        //Create classifierValue node
        var classifierValue = new XElement(mdcSchemaLocation + "classifierValue");
        var classifierValueCharacterString = new XElement(gcoNamespace + "CharacterString", parentClassifier.Name);
        classifierValue.Add(classifierValueCharacterString);
        classifier.Add(classifierValue);

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

    public string GetNodeValues(SearchableField field, XElement rootNode, XmlNamespaceManager nsMgr)
    {
        var value = string.Empty;

        if (field.Type == "list")
        {
            var elements = rootNode.XPathSelectElements(field.XPath, nsMgr);
            if (elements != null && elements.Any())
            {
                var values = elements.Select(x => x.Value).ToList();
                return string.Join(", ", values);
            }
        }
        else
        {
            var element = rootNode.XPathSelectElement(field.XPath, nsMgr);
            return element != null ? element.Value : string.Empty;
        }

        return value;
    }

    public void EnrichMetadataXmlWithNceaClassifiers(XmlNamespaceManager nsMgr, XElement rootNode, HashSet<Classifier> matchedClassifiers)
    {
        var ncClassifiersParentNode = GetNCClassifiersParentNode(rootNode, nsMgr);
        var nceaClassifiers = BuildClassifierHierarchies(matchedClassifiers.ToList());
        foreach (var nceaClassifier in nceaClassifiers)
        {
            var element = CreateClassifierNode(nceaClassifier, nceaClassifier.Children);
            ncClassifiersParentNode.Add(element);
        }
    }

    private static List<Classifier> BuildClassifierHierarchies(List<Classifier> flattenedClassifierList)
    {
        Action<Classifier> SetChildren = null!;

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

    public XmlNamespaceManager GetXmlNamespaceManager(XDocument xDoc)
    {
        var reader = xDoc.CreateReader();
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(reader.NameTable);
        nsMgr.AddNamespace("gmd", GmdNamespace);
        nsMgr.AddNamespace("gco", GcoNamespace);
        nsMgr.AddNamespace("gmx", GmxNamespace);
        nsMgr.AddNamespace("mdc", _mdcSchemaLocationPath);

        return nsMgr;
    }


    public XElement GetNCClassifiersParentNode(XElement rootNode, XmlNamespaceManager nsMgr)
    {
        nsMgr.AddNamespace("mdc", _mdcSchemaLocationPath);

        var classifierInfo = rootNode.XPathSelectElement("//mdc:nceaClassifierInfo", nsMgr);
        if (classifierInfo != null)
        {
            return classifierInfo.Elements().FirstOrDefault()!;
        }

        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;
        var nceaClassifierInfo = new XElement(mdcSchemaLocation + "nceaClassifierInfo");
        var nc_Classifiers = new XElement(mdcSchemaLocation + "NC_Classifiers");
        nceaClassifierInfo.Add(nc_Classifiers);
        rootNode.Add(nceaClassifierInfo);
        return nc_Classifiers;
    }
}
