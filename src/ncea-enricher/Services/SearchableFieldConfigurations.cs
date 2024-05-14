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
    public List<Field> GetAll()
    {
        var mdcFields = _configuration.GetSection("MdcFields").Get<List<Field>>()!;
        
        return mdcFields
            .Where(x => x.UsedForNceaProfiling == true)
            .ToList();
    }
}
