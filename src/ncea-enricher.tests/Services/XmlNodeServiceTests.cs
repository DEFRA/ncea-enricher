using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ncea.Enricher.Enums;
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
        _xmlNamespaceManager = GetXmlNamespaceManager();

        _serviceProvider = ServiceProviderForTests.Get();
    }

    [Fact]
    public void CreateClassifierNode_WhenNoChildClassifiersAreNull_ThenCreateOnlyTheParentNode()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);

        // Act
        var result = xmlNodeService.CreateClassifierNode(new Classifier { Id = "lvl1-001", Level = 1, Name = "test-value" }, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierName = result.XPathSelectElement("//mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName.Should().NotBeNull();
        classifierName!.Value.Should().Be("test-value");

        var classifierCode = result.XPathSelectElement("//mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode.Should().NotBeNull();
        classifierCode!.Value.Should().Be("lvl1-001");

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
            new Classifier { Id = "lvl2-001", Level = 2, Name = "test-value-2" }
        };

        // Act
        var result = xmlNodeService.CreateClassifierNode(new Classifier { Id = "lvl1-001", Level = 1, Name = "test-value" }, level2Classifiers);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierName = result.XPathSelectElement("//mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName.Should().NotBeNull();
        classifierName!.Value.Should().Be("test-value");

        var classifierCode = result.XPathSelectElement("//mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode.Should().NotBeNull();
        classifierCode!.Value.Should().Be("lvl1-001");

        var ncClassifiers = result.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        var classifierName2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName2.Should().NotBeNull();
        classifierName2!.Value.Should().Be("test-value-2");

        var classifierCode2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode2.Should().NotBeNull();
        classifierCode2!.Value.Should().Be("lvl2-001");
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
                Id = "lvl2-001",
                Level = 2, 
                Name = "test-value-2", 
                Children = new List<Classifier> 
                {
                    new Classifier 
                    {
                        Id = "lvl3-001",
                        Level = 3,
                        Name = "test-value-3"
                    }
                } 
            }
        };

        // Act
        var result = xmlNodeService.CreateClassifierNode(new Classifier { Id = "lvl1-001", Level = 1, Name = "test-value" }, level2AndLevel3Classifiers);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();

        var classifierName = result.XPathSelectElement("//mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName.Should().NotBeNull();
        classifierName!.Value.Should().Be("test-value");

        var classifierCode = result.XPathSelectElement("//mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode.Should().NotBeNull();
        classifierCode!.Value.Should().Be("lvl1-001");

        var ncClassifiers = result.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        var classifierName2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName2.Should().NotBeNull();
        classifierName2!.Value.Should().Be("test-value-2");

        var classifierCode2 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode2.Should().NotBeNull();
        classifierCode2!.Value.Should().Be("lvl2-001");

        var ncClassifiers1 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers1.Should().NotBeNull();

        var classifierName3 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers/mdc:classifier/mdc:name/gco:CharacterString", _xmlNamespaceManager);
        classifierName3.Should().NotBeNull();
        classifierName3!.Value.Should().Be("test-value-3");

        var classifierCode3 = result.XPathSelectElement("//mdc:NC_Classifiers/mdc:classifier/mdc:NC_Classifiers/mdc:classifier/mdc:code/gco:CharacterString", _xmlNamespaceManager);
        classifierCode3.Should().NotBeNull();
        classifierCode3!.Value.Should().Be("lvl3-001");
    }

    [Fact]
    public void GetNodeValues_WhenTextValueExists_ReturnTextValue()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var field = new Field
        {
            Name = FieldName.Title,
            Type =  FieldType.Text,
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:title/gco:CharacterString"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!);

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
        var field = new Field
        {
            Name = FieldName.Title,
            Type = FieldType.Text,
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:citation/gmd:CI_Citation/gmd:title/gco:CharacterString1"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!);

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
        var field = new Field
        {
            Name = FieldName.Title,
            Type = FieldType.List,
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:descriptiveKeywords/gmd:MD_Keywords/gmd:keyword/gmx:Anchor"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!);

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
        var field = new Field
        {
            Name = FieldName.Title,
            Type = FieldType.List,
            XPath = "//gmd:identificationInfo/gmd:MD_DataIdentification/gmd:descriptiveKeywords/gmd:MD_Keywords/gmd:keyword/gmx:Anchor1"
        };

        // Act
        var result = xmlNodeService.GetNodeValues(field, _xDoc.Root!);

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
        var result = xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!);

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
        xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!);

        // Act
        var result = xmlNodeService.GetNCClassifiersParentNode(_xDoc.Root!);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<XElement>();
    }

    [Fact]
    public void EnrichMetadataXmlWithNceaClassifiers_WhenNoMatchedClassifierExists_UpdateNCClassiferNodeWithoutClassifiers()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var matchedClassifier = new HashSet<Classifier>();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = XDocument.Load(filePath);

        // Act
        xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(xDoc.Root!, matchedClassifier);

        // Assert
        var nceaClassifierInfo = xDoc.XPathSelectElement("//mdc:nceaClassifierInfo", _xmlNamespaceManager);
        nceaClassifierInfo.Should().NotBeNull();

        var ncClassifiers = xDoc.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        ncClassifiers!.Elements().Count().Should().Be(0);
    }

    [Fact]
    public void EnrichMetadataXmlWithNceaClassifiers_WhenMatchedClassifierExists_UpdateNCClassiferNodeWithClassifiers()
    {
        // Arrange
        var configuration = _serviceProvider.GetService<IConfiguration>();
        var xmlNodeService = new XmlNodeService(configuration!);
        var matchedClassifier = new HashSet<Classifier>
        {
            new Classifier { ParentId = null, Id = "test-id-1", Level = 1, Name = "test-value-1" },
            new Classifier { ParentId = "test-id-1", Id = "test-id-2", Level = 2, Name = "test-value-2" },
            new Classifier { ParentId = "test-id-2", Id = "test-id-3", Level = 3, Name = "test-value-3" },
            new Classifier { ParentId = null, Id = "test-id-4", Level = 1, Name = "test-value-4" },
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "MEDIN_Metadata_series_v3_1_2_example 1.xml");
        var xDoc = XDocument.Load(filePath);

        // Act
        xmlNodeService.EnrichMetadataXmlWithNceaClassifiers(xDoc.Root!, matchedClassifier);

        // Assert
        var nceaClassifierInfo = xDoc.XPathSelectElement("//mdc:nceaClassifierInfo", _xmlNamespaceManager);
        nceaClassifierInfo.Should().NotBeNull();

        var ncClassifiers = xDoc.XPathSelectElement("//mdc:NC_Classifiers", _xmlNamespaceManager);
        ncClassifiers.Should().NotBeNull();

        ncClassifiers!.Elements().Count().Should().Be(2);
    }

    private static XmlNamespaceManager GetXmlNamespaceManager()
    {        
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
        nsMgr.AddNamespace("gmd", "http://www.isotc211.org/2005/gmd");
        nsMgr.AddNamespace("gco", "http://www.isotc211.org/2005/gco");
        nsMgr.AddNamespace("gmx", "http://www.isotc211.org/2005/gmx");
        nsMgr.AddNamespace("mdc", "https://github.com/DEFRA/ncea-geonetwork/tree/main/core-geonetwork/schemas/iso19139/src/main/plugin/iso19139/schema2007/mdc");

        return nsMgr;
    }
}
