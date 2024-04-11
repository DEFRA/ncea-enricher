using Ncea.Enricher.Services.Contracts;
using System.Text.RegularExpressions;

namespace Ncea.Enricher.Services
{
    public class XmlSearchService : IXmlSearchService
    {
        public bool IsMatchFound(string value, List<string> synonyms)
        {
            var rgx = new Regex(@"\b" + string.Join("|", synonyms.Select(Regex.Escape).ToArray()) + @"\b");
            var matchCollection = rgx.Matches(value);
            return matchCollection.Count > 0;
        }
    }
}
