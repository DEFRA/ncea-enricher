﻿using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Ncea.Enricher.Services;

[ExcludeFromCodeCoverage]
public class XsdValidationService : IXmlValidationService
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";
    private const string GmlSchema = "https://schemas.opengis.net/gml/3.2.1/gml.xsd";
    private const string MdcSchema = "https://ncea-search.azure.defra.cloud/mdc";

    private List<string>? _errorList;
    private readonly XmlSchemaSet _schemas;
    private readonly ILogger<XsdValidationService> _logger;

    public XsdValidationService(ILogger<XsdValidationService> logger)
    {        
        _logger = logger;

        _schemas = new XmlSchemaSet();
        _schemas.XmlResolver = new XmlUrlResolver();
        _schemas.ValidationEventHandler += XmlSchemaSetValidationEventHandler!;
        _schemas.Add(GmdNamespace, Path.Combine(GmdNamespace, "gmd.xsd"));
        _schemas.Add(GcoNamespace, Path.Combine(GcoNamespace, "gco.xsd"));
        _schemas.Add(GmxNamespace, Path.Combine(GmxNamespace, "gmx.xsd"));
        _schemas.Add(GmlNamespace, GmlSchema);
        _schemas.Add(MdcSchema, Path.Combine("Schema", "mdc", "mdc.xsd"));
        _schemas.Compile();
    }

    public void Validate(XDocument xDoc, string dataSource, string fileIdentifier)
    {
        _errorList = [];
        xDoc.Validate(_schemas, ValidationEventHandler!);

        if (_errorList.Count != 0)
        {           
            var errorMessage = $"MDC Schema/Data mismatch detected on xml with FileIdentifier : {fileIdentifier}, DataSource : {dataSource}";
            CustomLogger.LogWarningMessage(_logger, errorMessage, null);
        }
    }

    private void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (Enum.TryParse<XmlSeverityType>("Error", out XmlSeverityType type) && type == XmlSeverityType.Error)
        {
            _errorList!.Add(e.Message);
        }
    }

    /// <summary>
    /// Ignore the global attribute already declared errors.
    /// </summary>
    private void XmlSchemaSetValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (!e.Exception.Message.Contains("has already been declared") 
            && !e.Exception.SourceUri!.Contains("www.isotc211.org/2005/gml/coverage.xsd"))
        {
            CustomLogger.LogErrorMessage(_logger, "XML Schema Already exists", e.Exception);
        }
    }
}
