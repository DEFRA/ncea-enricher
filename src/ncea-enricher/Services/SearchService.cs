using Ncea.Enricher.Services.Contracts;
using System.Text.RegularExpressions;

namespace Ncea.Enricher.Services
{
    public class SearchService : ISearchService
    {
        public bool IsMatchFound(string value, List<string> synonyms)
        {
            var rgx = new Regex(@"\b" + string.Join("|", synonyms.Select(Regex.Escape).ToArray()) + @"\b", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000));
            var matchCollection = rgx.Matches(value);
            return matchCollection.Count > 0;
        }

        public bool IsMatchFound(List<string> values, List<string> synonyms)
        {
            var rgx = new Regex(@"\b" + string.Join("|", synonyms.Select(Regex.Escape).ToArray()) + @"\b", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(1000));
            foreach( var value in values )
            {
                var matchCollection = rgx.Matches(value);
                if( matchCollection.Count > 0 ) 
                { 
                    return true; 
                }
            }
            return false;
        }
    }
}
