using Ncea.Enricher.Models;
using Ncea.Enricher.Services.Contracts;

namespace Ncea.Enricher.Services;

public class MdcFieldConfigurationService : IMdcFieldConfigurationService
{
    private readonly IConfiguration _configuration;

    public MdcFieldConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public List<Field> GetAll()
    {
        var mdcFields = _configuration.GetSection("MdcFields").Get<List<Field>>()!;
        
        return mdcFields
            .ToList();
    }

    public List<Field> GetFieldsForClassification()
    {
        var mdcFields = _configuration.GetSection("MdcFields").Get<List<Field>>()!;

        return mdcFields
            .Where(x => x.UsedForNceaProfiling == true)
            .ToList();
    }
}
