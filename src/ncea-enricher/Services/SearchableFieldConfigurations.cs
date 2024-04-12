using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class SearchableFieldConfigurations : ISearchableFieldConfigurations
{
    private readonly IConfiguration _configuration;

    public SearchableFieldConfigurations(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public List<SearchableField> GetAll()
    {
        return _configuration.GetSection("SearchableFields").Get<List<SearchableField>>()!;
    }
}
