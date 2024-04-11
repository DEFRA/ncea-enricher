using Ncea.Enricher.Services.Contracts;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Ncea.Enricher.Services
{
    public class XmlSearchService : IXmlSearchService
    {
        public bool IsMatchFound(XElement root, string elementPath, List<string> synonyms)
        {
            var rgx = new Regex(@"\b(" + string.Join("|", synonyms.Select(Regex.Escape).ToArray()) + @"\b)");
            var element = ElementAtPath(root, elementPath);
            var matchCollection = rgx.Matches(element.Value);
            return matchCollection.Count > 0;
        }

        public bool IsMatchFound(string value, List<string> synonyms)
        {
            var rgx = new Regex(@"\b" + string.Join("|", synonyms.Select(Regex.Escape).ToArray()) + @"\b");
            var matchCollection = rgx.Matches(value);
            return matchCollection.Count > 0;
        }

        private static XElement ElementAtPath(XElement root, string path)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid path.");
            }

            return root.XPathSelectElement(path)!;
        }
    }
}
