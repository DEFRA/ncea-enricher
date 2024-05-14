using Ncea.Enricher.Enums;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Ncea.Enricher.Services;

[ExcludeFromCodeCoverage]
public class XPathValidationService : IXmlValidationService
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";
    private const string MdcNamespace = "https://github.com/DEFRA/ncea-geonetwork/tree/main/core-geonetwork/schemas/iso19139/src/main/plugin/iso19139/schema2007/mdc";

    private readonly List<Field> _mdcFields;

    public XPathValidationService(IConfiguration configuration)
    {
        _mdcFields = configuration.GetSection("MdcFields").Get<List<Field>>()!;
    }

    public void Validate(XDocument xDoc)
    {
        var errorList = new List<string>();
        var rootNode = xDoc.Root;

        var reader = xDoc.CreateReader();
        var nsMgr = new XmlNamespaceManager(reader.NameTable);
        nsMgr.AddNamespace("gmd", GmdNamespace);
        nsMgr.AddNamespace("gco", GcoNamespace);
        nsMgr.AddNamespace("gmx", GmxNamespace);
        nsMgr.AddNamespace("gml", GmlNamespace);
        nsMgr.AddNamespace("mdc", MdcNamespace);

        var resourceType = GetResourceType(rootNode!, nsMgr);
        if (resourceType == null)
        {
            throw new XmlSchemaValidationException();
        }

        ValidateMandatoryFields(errorList, rootNode!, nsMgr, resourceType.Value);
    }

    private void ValidateMandatoryFields(List<string> errorList, XElement rootNode, XmlNamespaceManager nsMgr, ResourceType resourceType)
    {
        var fieldsToBeValidated = _mdcFields
            .Where(x  => x.Obligation == MdcObligation.Mandatory && x.RelevantFor.Contains(resourceType))
            .ToList();

        foreach (var field in fieldsToBeValidated)
        {
            var fieldNameText = field.Name.ToString();
            if (field.Type == FieldType.List)
            {
                CheckListField(rootNode!, nsMgr, errorList, field, fieldNameText);
            }
            else if (field.Type == FieldType.ConditionalText)
            {
                CheckConditionalTextField(rootNode!, nsMgr, errorList, field, fieldNameText);
            }
            else
            {
                CheckTextField(rootNode!, nsMgr, errorList, field, fieldNameText);
            }
        }
    }

    private static void CheckConditionalTextField(XElement rootNode, XmlNamespaceManager nsMgr, List<string> errorList, Field field, string fieldNameText)
    {
        var parentElement = rootNode!.XPathSelectElement(field.XPath, nsMgr);
        if (parentElement != null)
        {
            var conditionalElement = parentElement.XPathSelectElement(field.ConditionalChild, nsMgr);
            var relatedElement = parentElement.XPathSelectElement(field.RelatedChild, nsMgr);
            if (conditionalElement != null && relatedElement != null)
            {
                if (string.IsNullOrWhiteSpace(conditionalElement.Value) || string.IsNullOrWhiteSpace(relatedElement.Value))
                {
                    errorList.Add(fieldNameText);
                }
                errorList.Add(fieldNameText);
            }
            else
            {
                errorList.Add(fieldNameText);
            }
        }
        else
        {
            errorList.Add(fieldNameText);
        }
    }

    private static void CheckTextField(XElement rootNode, XmlNamespaceManager nsMgr, List<string> errorList, Field field, string fieldNameText)
    {
        var element = rootNode.XPathSelectElement(field.XPath, nsMgr);
        if (element != null)
        {
            if (string.IsNullOrWhiteSpace(element.Value))
            {
                errorList.Add(fieldNameText);
            }
        }
        else
        {
            errorList.Add(fieldNameText);
        }
    }

    private static void CheckListField(XElement rootNode, XmlNamespaceManager nsMgr, List<string> errorList, Field field, string fieldNameText)
    {
        var elements = rootNode.XPathSelectElements(field.XPath, nsMgr);
        if (elements != null)
        {
            if (elements.Any())
            {
                errorList.Add(fieldNameText);
            }
        }
        else
        {
            errorList.Add(fieldNameText);
        }
    }

    private ResourceType? GetResourceType(XElement rootNode, XmlNamespaceManager nsManager)
    {
        var xpathResourceType = "//gmd:hierarchyLevel/gmd:MD_ScopeCode";
        var resourceTypeNode = rootNode.XPathSelectElement(xpathResourceType, nsManager);
        return resourceTypeNode != null ? Enum.Parse<ResourceType>(resourceTypeNode.Value, true) : null;
    }
}
