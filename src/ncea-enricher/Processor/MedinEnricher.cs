using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;
using System.Xml;
using System.Xml.Linq;

namespace Ncea.Enricher.Processors;

public class MedinEnricher : IEnricherService
{
    private readonly ISynonymsProvider _synonymsProvider;
    private readonly ISearchableFieldConfigurations _searchableFieldConfigurations;
    private readonly IXmlSearchService _xmlSearchService;
    private readonly IXmlNodeService _xmlNodeService;
    private readonly ILogger<MedinEnricher> _logger;

    public MedinEnricher(ISynonymsProvider synonymsProvider,
        ISearchableFieldConfigurations searchableFieldConfigurations,
        IXmlSearchService xmlSearchService,
        IXmlNodeService xmlNodeService,
        ILogger<MedinEnricher> logger)
    {
        _synonymsProvider = synonymsProvider;
        _searchableFieldConfigurations = searchableFieldConfigurations;
        _xmlSearchService = xmlSearchService;
        _xmlNodeService = xmlNodeService;
        _logger = logger;
    }
    public async Task<string> Enrich(string mappedData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Medin enricher");
        
        var searchableFieldValues = new Dictionary<string, string>();

        var xDoc = XDocument.Parse(mappedData);
        var nsMgr = GetXmlNamespaceManager(xDoc);
        var rootNode = xDoc.Root!;

        var searchableFields = _searchableFieldConfigurations.GetSearchableFieldConfigurations();
        foreach(var searchableField in searchableFields)
        {
            var fieldValue = _xmlNodeService.GetNodeValues(searchableField, rootNode, nsMgr);
            searchableFieldValues.Add(searchableField.Name, fieldValue);
        }

        var classifierList = await _synonymsProvider.GetAll(cancellationToken);
        var classifiers = classifierList.Where(x => x.Synonyms != null).ToList();
        var metadata = searchableFieldValues.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Value).ToList();

        var matchedClassifiers = new HashSet<Classifier>();
        foreach(var classifier in classifiers)
        {
            foreach (var data in metadata)
            {
                if(_xmlSearchService.IsMatchFound(data, classifier.Synonyms!))
                {
                    CollectRelatedClassifiers(matchedClassifiers, classifierList, classifier);
                    break;
                }
            }
        }        

        var ncClassifiersParentNode = _xmlNodeService.GetNCClassifiersParentNode(rootNode, nsMgr);
        var nceaClassifiers = BuildClassifierHierarchies(matchedClassifiers.ToList());
        foreach (var nceaClassifier in nceaClassifiers)
        {
            var element = _xmlNodeService.CreateClassifierNode(nceaClassifier.Level, nceaClassifier.Name, nceaClassifier.Children);
            ncClassifiersParentNode.Add(element);
        }

        return await Task.FromResult(xDoc.ToString());
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

    private static XmlNamespaceManager GetXmlNamespaceManager(XDocument xDoc)
    {
        var reader = xDoc.CreateReader();
        XmlNamespaceManager nsMgr = new XmlNamespaceManager(reader.NameTable);
        nsMgr.AddNamespace("gmd", "http://www.isotc211.org/2005/gmd");
        nsMgr.AddNamespace("gco", "http://www.isotc211.org/2005/gco");
        nsMgr.AddNamespace("gmx", "http://www.isotc211.org/2005/gmx");

        return nsMgr;
    }

    private static void CollectRelatedClassifiers(HashSet<Classifier> matchedClassifiers, List<Classifier> classifierList, Classifier classifier)
    {
        matchedClassifiers.Add(classifier);
        while (classifier.ParentId != null)
        {
            classifier = classifierList.Single(x => x.Id == classifier.ParentId);
            matchedClassifiers.Add(classifier);
        }
    }
}
