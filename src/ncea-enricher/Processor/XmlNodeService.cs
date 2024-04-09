using ncea.enricher.Processor.Contracts;
using Ncea.Enricher.Models;
using System.Xml.Linq;

namespace ncea.enricher.Processor;

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
            foreach( var classifer in classifers)
            {
                nc_ClassifiersChild.Add(CreateClassifierNode(classifer.Level, classifer.Name, classifer.Children));
            }          
            
            classifier.Add(nc_ClassifiersChild);
        }        

        return classifier;
    }
}
