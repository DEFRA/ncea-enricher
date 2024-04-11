using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Services;

public class XmlNodeService : IXmlNodeService
{
    private readonly string _mdcSchemaLocationPath;

    public XmlNodeService(IConfiguration configuration)
    {
        _mdcSchemaLocationPath = configuration.GetValue<string>("MdcSchemaLocation")!;
    }

    public XElement CreateClassifierNode(int level, string value, List<Classifier>? classifers)
    {
        XNamespace gcoNamespace = "http://www.isotc211.org/2005/gco";
        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;

        var classifier = new XElement(mdcSchemaLocation + "classifier");

        //Create classifierType node
        var classifierType = new XElement(mdcSchemaLocation + "classifierType");
        var classifierTypeCharacterString = new XElement(gcoNamespace + "CharacterString", string.Format("Level {0}", level));
        classifierType.Add(classifierTypeCharacterString);
        classifier.Add(classifierType);

        //Create classifierValue node
        var classifierValue = new XElement(mdcSchemaLocation + "classifierValue");
        var classifierValueCharacterString = new XElement(gcoNamespace + "CharacterString", value);
        classifierValue.Add(classifierValueCharacterString);
        classifier.Add(classifierValue);

        if (classifers != null && classifers.Count != 0)
        {
            //Create child nc_Classifiers node
            var nc_ClassifiersChild = new XElement(mdcSchemaLocation + "NC_Classifiers");
            foreach (var classifer in classifers)
            {
                nc_ClassifiersChild.Add(CreateClassifierNode(classifer.Level, classifer.Name, classifer.Children));
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
            if (elements != null && elements.Count() > 0)
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

    public XElement GetNCClassifiersParentNode(XElement rootNode, XmlNamespaceManager nsMgr)
    {
        nsMgr.AddNamespace("mdc", _mdcSchemaLocationPath);

        var classifierInfo = rootNode.XPathSelectElement("//mdc:nceaClassifierInfo", nsMgr);
        var classifiers = rootNode.XPathSelectElement("//mdc:nceaClassifierInfo/NC_Classifiers", nsMgr);
        if (classifierInfo != null && classifiers != null)
        {
            return classifiers;
        }

        XNamespace mdcSchemaLocation = _mdcSchemaLocationPath;
        var nceaClassifierInfo = new XElement(mdcSchemaLocation + "nceaClassifierInfo");
        var nc_Classifiers = new XElement(mdcSchemaLocation + "NC_Classifiers");
        nceaClassifierInfo.Add(nc_Classifiers);
        rootNode.Add(nceaClassifierInfo);
        return nc_Classifiers;
    }
}
