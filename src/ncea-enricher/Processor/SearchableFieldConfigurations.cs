using Ncea.Enricher.Models;
using Ncea.Enricher.Processor.Contracts;

namespace ncea.enricher.Processor;

public class SearchableFieldConfigurations : ISearchableFieldConfigurations
{
    private readonly IConfiguration _configuration;

    public SearchableFieldConfigurations(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public List<SearchableField> GetSearchableFieldConfigurations()
    {
        return _configuration.GetSection("SearchableFields").Get<List<SearchableField>>()!;
    }
}
