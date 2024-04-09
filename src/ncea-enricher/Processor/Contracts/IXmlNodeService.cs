using Ncea.Enricher.Models;
using System.Xml.Linq;

namespace ncea.enricher.Processor.Contracts;

public interface IXmlNodeService
{
    XElement CreateClassifierNode(int level, string value, List<Classifier>? classifers);
}
