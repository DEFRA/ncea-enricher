using Ncea.Enricher.Constants;
using Ncea.Enricher.Enums;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Utils;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Services;

public class XPathValidationService : IXmlValidationService
{
    private readonly List<Field> _mdcFields;
    private readonly XmlNamespaceManager _nsMgr;
    private readonly ILogger<XPathValidationService> _logger;

    public XPathValidationService(IConfiguration configuration, ILogger<XPathValidationService> logger)
    {
        _logger = logger;        
        _mdcFields = configuration.GetSection("MdcFields").Get<List<Field>>()!;

        var mdcNamespace= configuration.GetValue<string>("MdcSchemaLocation")!;
        _nsMgr = new XmlNamespaceManager(new NameTable());
        _nsMgr.AddNamespace("gmd", XmlNamespaces.Gmd);
        _nsMgr.AddNamespace("gco", XmlNamespaces.Gco);
        _nsMgr.AddNamespace("gmx", XmlNamespaces.Gmx);
        _nsMgr.AddNamespace("gml", XmlNamespaces.Gml);
        _nsMgr.AddNamespace("srv", XmlNamespaces.Srv);
        _nsMgr.AddNamespace("mdc", mdcNamespace);
    }

    public void Validate(XDocument xDoc, string dataSource, string fileIdentifier)
    {
        var errorList = new List<string>();
        var rootNode = xDoc.Root;

        var resourceType = GetResourceType(rootNode!, _nsMgr);
        if (resourceType != null)
        {
            ValidateMandatoryFields(errorList, rootNode!, resourceType.Value);
        }

        if(errorList.Count != 0)
        {
            var dataMismatchFieldNames = string.Join(", ", errorList.ToArray());
            var errorMessage = $"MDC Schema/Data mismatch detected on the mandatory fields : '{dataMismatchFieldNames}' of xml with FileIdentifier : {fileIdentifier}, DataSource : {dataSource}";
            CustomLogger.LogWarningMessage(_logger, errorMessage, null);
        }
    }

    private void ValidateMandatoryFields(List<string> errorList, XElement rootNode, ResourceType resourceType)
    {
        var fieldsToBeValidated = _mdcFields
            .Where(x  => x.Obligation == MdcObligation.Mandatory && x.RelevantFor.Contains(resourceType))
            .ToList();

        foreach (var field in fieldsToBeValidated)
        {
            var fieldNameText = field.Name.ToString();
            if (field.Type == FieldType.List)
            {
                CheckListField(rootNode!, errorList, field, fieldNameText);
            }
            else if (field.Type == FieldType.ConditionalText)
            {
                CheckConditionalTextField(rootNode!, errorList, field, fieldNameText);
            }            
            else
            {
                CheckTextField(rootNode!, errorList, field, fieldNameText);
            }
        }
    }

    private void CheckConditionalTextField(XElement rootNode, List<string> errorList, Field field, string fieldNameText)
    {
        var parentElement = rootNode!.XPathSelectElement(field.XPath, _nsMgr);
        if (parentElement != null)
        {
            var conditionalElement = parentElement.XPathSelectElement(field.ConditionalChild, _nsMgr);
            var relatedElement = parentElement.Parent!.XPathSelectElement(field.RelatedChild, _nsMgr);
            if (conditionalElement != null && relatedElement != null)
            {
                if (string.IsNullOrWhiteSpace(conditionalElement.Value) || string.IsNullOrWhiteSpace(relatedElement.Value))
                {
                    errorList.Add(fieldNameText);
                }
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

    private void CheckTextField(XElement rootNode, List<string> errorList, Field field, string fieldNameText)
    {
        var element = rootNode.XPathSelectElement(field.XPath, _nsMgr);
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

    private void CheckListField(XElement rootNode, List<string> errorList, Field field, string fieldNameText)
    {
        var elements = rootNode.XPathSelectElements(field.XPath, _nsMgr);
        if (elements != null)
        {
            if (!elements.Any())
            {
                errorList.Add(fieldNameText);
            }
        }
        else
        {
            errorList.Add(fieldNameText);
        }
    }

    private static ResourceType? GetResourceType(XElement rootNode, XmlNamespaceManager nsManager)
    {
        var xpathResourceType = "//gmd:hierarchyLevel/gmd:MD_ScopeCode";
        var resourceTypeNode = rootNode.XPathSelectElement(xpathResourceType, nsManager);
        return resourceTypeNode != null ? Enum.Parse<ResourceType>(resourceTypeNode.Value, true) : null;
    }
}
