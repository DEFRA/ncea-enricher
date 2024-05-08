using Ncea.Enricher.BusinessExceptions;
using Ncea.Enricher.Services.Contracts;
using Ncea.Enricher.Utils;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ncea.enricher.Services;

public class XmlValidationService : IXmlValidationService
{
    private const string GmdNamespace = "http://www.isotc211.org/2005/gmd";
    private const string GcoNamespace = "http://www.isotc211.org/2005/gco";
    private const string GmxNamespace = "http://www.isotc211.org/2005/gmx";
    private const string GmlNamespace = "http://www.opengis.net/gml/3.2";
    private const string GmlSchema = "https://schemas.opengis.net/gml/3.2.1/gml.xsd";

    private readonly XmlSchemaSet _schemas;
    private readonly ILogger<XmlValidationService> _logger;

    public XmlValidationService(ILogger<XmlValidationService> logger)
    {
        _logger = logger;

        _schemas = new XmlSchemaSet();
        _schemas.XmlResolver = new XmlUrlResolver();
        _schemas.ValidationEventHandler += XmlSchemaSetValidationEventHandler!;
        _schemas.Add(GmdNamespace, Path.Combine(GmdNamespace, "gmd.xsd"));
        _schemas.Add(GcoNamespace, Path.Combine(GcoNamespace, "gco.xsd"));
        _schemas.Add(GmxNamespace, Path.Combine(GmxNamespace, "gmx.xsd"));
        _schemas.Add(GmlNamespace, GmlSchema);
        _schemas.Compile();
    }

    public void Validate(XDocument xDoc)
    {
        xDoc.Validate(_schemas, ValidationEventHandler!);
    }

    private void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (Enum.TryParse<XmlSeverityType>("Error", out XmlSeverityType type) && type == XmlSeverityType.Error)
        {
            throw new XmlValidationException(e.Message, e.Exception);
        }
    }

    /// <summary>
    /// Ignore the global attribute already declared errors.
    /// </summary>
    private void XmlSchemaSetValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (!e.Exception.Message.Contains("has already been declared") 
            && e.Exception.SourceUri!.Contains("www.isotc211.org/2005/gml/coverage.xsd"))
        {
            CustomLogger.LogErrorMessage(_logger, "XML Schema Already exists", e.Exception);
        }
    }
}
