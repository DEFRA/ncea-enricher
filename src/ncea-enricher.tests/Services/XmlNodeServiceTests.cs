using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Models;
using Ncea.Enricher.Services;
using Ncea.Enricher.Tests.Clients;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Tests.Services;

public class XmlNodeServiceTests
{
    private IServiceProvider _serviceProvider;
    private XDocument _xDoc;
    private XmlNamespaceManager _xmlNamespaceManager;

    public XmlNodeServiceTests()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        _xDoc = XDocument.Load(filePath);

        _serviceProvider = ServiceProviderForTests.Get();
        _xmlNamespaceManager = GetXmlNamespaceManager();
    }

    [Fact]
    public void CreateClassifierNode_WhenNoChildClassifiersAreNull_ThenCreateOnlyTheParentNode()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);

        // Act
        var result = xmlNodeService.CreateClassifierNode(1, "test-value", null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierLevel = result.XPathSelectElement("//mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel.Should().NotBeNull();
        classifierLevel!.Value.Should().Be("Level 1");

        var classifierValue = result.XPathSelectElement("//mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue.Should().NotBeNull();
        classifierValue!.Value.Should().Be("test-value");

        var ncClassifiers = result.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().BeNull();
    }

    [Fact]
    public void CreateClassifierNode_WhenLevel2ChildClassifiersExists_ThenCreateLevel1AndLevel2HiererchyNodes()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var level2Classifiers = new List<Classifier>
        {
            new Classifier { Level = 2, Name = "test-value-2" }
        };

        // Act
        var result = xmlNodeService.CreateClassifierNode(1, "test-value", level2Classifiers);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierLevel = result.XPathSelectElement("//mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel.Should().NotBeNull();
        classifierLevel!.Value.Should().Be("Level 1");

        var classifierValue = result.XPathSelectElement("//mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue.Should().NotBeNull();
        classifierValue!.Value.Should().Be("test-value");

        var ncClassifiers = result.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        var classifierLevel2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel2.Should().NotBeNull();
        classifierLevel2!.Value.Should().Be("Level 2");

        var classifierValue2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue2.Should().NotBeNull();
        classifierValue2!.Value.Should().Be("test-value-2");
    }

    [Fact]
    public void CreateClassifierNode_WhenLevel2AndLevel3ChildClassifiersExists_ThenCreateLevel1Level2AndLevel3HiererchyNodes()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var level2AndLevel3Classifiers = new List<Classifier>
        {
            new Classifier { 
                Level = 2, 
                Name = "test-value-2", 
                Children = new List<Classifier> 
                {
                    new Classifier 
                    {
                        Level = 3,
                        Name = "test-value-3"
                    }
                } 
            }
        };

        // Act
        var result = xmlNodeService.CreateClassifierNode(1, "test-value", level2AndLevel3Classifiers);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierLevel = result.XPathSelectElement("//mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel.Should().NotBeNull();
        classifierLevel!.Value.Should().Be("Level 1");

        var classifierValue = result.XPathSelectElement("//mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue.Should().NotBeNull();
        classifierValue!.Value.Should().Be("test-value");

        var ncClassifiers = result.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        var classifierLevel2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel2.Should().NotBeNull();
        classifierLevel2!.Value.Should().Be("Level 2");

        var classifierValue2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue2.Should().NotBeNull();
        classifierValue2!.Value.Should().Be("test-value-2");

        var ncClassifiers1 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers1.Should().NotBeNull();

        var classifierLevel3 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers/mdc:classifier/mdc:classifierType/gco:CharacterString", _xmlNamespaceManager);
        classifierLevel3.Should().NotBeNull();
        classifierLevel3!.Value.Should().Be("Level 3");

        var classifierValue3 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers/mdc:classifier/mdc:classifierValue/gco:CharacterString", _xmlNamespaceManager);
        classifierValue3.Should().NotBeNull();
        classifierValue3!.Value.Should().Be("test-value-3");
    }

    [Fact]
    public void GetNodeValues_WhenTextValueExists_ReturnTextValue()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var field = new SearchableField
        {
            Name = "Title",
            Type = "text",
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:title/gco:CharacterString"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public void GetNodeValues_WhenTextValueNotExists_ReturnEmptyString()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var field = new SearchableField
        {
            Name = "Title",
            Type = "text",
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:title/gco:CharacterString1"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetNodeValues_WhenListValueExists_ReturnCommaSeperatedTextValues()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var field = new SearchableField
        {
            Name = "Title",
            Type = "list",
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:descriptiveKeywords/gmd:MD_Keywords/gmd:keyword/gmx:Anchor"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().NotBeEmpty();
        result.Should().Contain(",");
    }

    [Fact]
    public void GetNodeValues_WhenListValueNotExists_ReturnEmptyString()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var field = new SearchableField
        {
            Name = "Title",
            Type = "list",
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:descriptiveKeywords/gmd:MD_Keywords/gmd:keyword/gmx:Anchor1"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<string>();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetNCClassifiersParentNode_WhenNCClassifiersNodeNotExists_ThenCreateNewNode()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);

        // Act
        var result = xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();
    }

    [Fact]
    public void GetNCClassifiersParentNode_WhenNCClassifiersNodeExists_ThenCreateExistingNode()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!, _xmlNamespaceManager);

        // Act
        var result = xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!, _xmlNamespaceManager);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();
    }
    private XmlNamespaceManager GetXmlNamespaceManager()
    {
        var reader = _xDoc.CreateReader();
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(reader.NameTable);
        nsMgr.AddNamespace("gmd", "http://www.isotc211.org/2005/gmd");
        nsMgr.AddNamespace("gco", "http://www.isotc211.org/2005/gco");
        nsMgr.AddNamespace("gmx", "http://www.isotc211.org/2005/gmx");
        nsMgr.AddNamespace("mdc", "https://github.com/DEFRA/ncea-geonetwork/tree/main/core-geonetwork/schemas/iso19139/src/main/plugin/iso19139/schema2007/mdc");

        return nsMgr;
    }
}
